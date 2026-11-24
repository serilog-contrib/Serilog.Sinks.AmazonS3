// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AmazonS3Sink.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
// This class is the main class and contains all logic for the AmazonS3 sink.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3;

using IBatchedLogEventSink = PeriodicBatching.IBatchedLogEventSink;

/// <summary>
/// This class is the main class and contains all logic for the AmazonS3 sink.
/// Amazon S3 objects cannot be appended to, so the batches of a rolling interval are collected in a local file next to
/// the configured path and that file is uploaded once the interval is over, or when the sink is disposed.
/// </summary>
public class AmazonS3Sink : IBatchedLogEventSink, IDisposable
{
    /// <summary>
    /// The Amazon S3 options.
    /// </summary>
    private readonly AmazonS3Options amazonS3Options = new();

    /// <summary>
    /// The time provider the rolling intervals are taken from.
    /// </summary>
    private readonly TimeProvider timeProvider;

    /// <summary>
    /// A value indicating whether the sink created the Amazon S3 client itself and therefore needs to dispose it.
    /// </summary>
    private bool ownsAmazonS3Client;

    /// <summary>
    /// The local file the events of the current rolling interval are appended to, <c>null</c> until the first batch.
    /// </summary>
    private string? currentFileName;

    /// <summary>
    /// The time before which no rolling interval can be over, <c>null</c> to check the local files at the next opportunity.
    /// </summary>
    private DateTime? nextUploadCheck;

    /// <summary>
    /// A value indicating whether the sink was disposed already.
    /// </summary>
    private bool disposed;

    /// <summary>Initializes a new instance of the <see cref="AmazonS3Sink" /> class. </summary>
    /// <exception cref="ArgumentNullException">A given value is null.</exception>
    /// <param name="amazonS3Options">The Amazon S3 options.</param>
    public AmazonS3Sink(AmazonS3Options amazonS3Options)
        : this(amazonS3Options, TimeProvider.System)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="AmazonS3Sink" /> class. </summary>
    /// <exception cref="ArgumentNullException">A given value is null.</exception>
    /// <param name="amazonS3Options">The Amazon S3 options.</param>
    /// <param name="timeProvider">The time provider the rolling intervals are taken from.</param>
    internal AmazonS3Sink(AmazonS3Options amazonS3Options, TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
        this.amazonS3Options.AmazonS3Client = amazonS3Options.AmazonS3Client;
        this.amazonS3Options.AwsAccessKeyId = amazonS3Options.AwsAccessKeyId;
        this.amazonS3Options.AwsSecretAccessKey = amazonS3Options.AwsSecretAccessKey;
        this.amazonS3Options.BucketName = amazonS3Options.BucketName;
        this.amazonS3Options.BucketPath = amazonS3Options.BucketPath;
        this.amazonS3Options.Encoding = amazonS3Options.Encoding;
        this.amazonS3Options.Endpoint = amazonS3Options.Endpoint;
        this.amazonS3Options.Path = amazonS3Options.Path;

        if (amazonS3Options.Formatter is null)
        {
            var textFormatter = new MessageTemplateTextFormatter(amazonS3Options.OutputTemplate!, amazonS3Options.FormatProvider);
            this.amazonS3Options.Formatter = textFormatter;
        }
        else
        {
            this.amazonS3Options.Formatter = amazonS3Options.Formatter;
        }

        this.amazonS3Options.ServiceUrl = amazonS3Options.ServiceUrl;
        this.amazonS3Options.PathRoller = new PathRoller(amazonS3Options.Path, amazonS3Options.RollingInterval);
        this.amazonS3Options.RollingInterval = amazonS3Options.RollingInterval;
        this.amazonS3Options.OutputTemplate = amazonS3Options.OutputTemplate;
        this.amazonS3Options.FormatProvider = amazonS3Options.FormatProvider;
        this.amazonS3Options.DisablePayloadSigning = amazonS3Options.DisablePayloadSigning;
    }

    /// <summary>
    /// Appends a batch of log events to the local file of the current rolling interval and uploads the local files of
    /// the rolling intervals that are over.
    /// </summary>
    /// <param name="batch">The batch of events to emit.</param>
    /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
    public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
    {
        var now = this.GetNow();

        // The batch is formatted before the file is touched. A formatter that throws leaves nothing behind then, and the
        // batch that is emitted again does not end up in the file twice.
        using var content = new StringWriter(CultureInfo.CurrentCulture);

        foreach (var logEvent in batch)
        {
            this.amazonS3Options.Formatter?.Format(logEvent, content);
        }

        var fileInformation = this.OpenFile(await this.GetCurrentFileName(now));

        if (fileInformation.OutputWriter is null)
        {
            throw new InvalidOperationException($"The output writer was not properly set for {fileInformation.FileName}");
        }

        try
        {
            await fileInformation.OutputWriter.WriteAsync(content.ToString());
            await fileInformation.OutputWriter.FlushAsync();
        }
        finally
        {
            // The writer needs to be closed in any case, otherwise a failing write leaves the file locked.
            await fileInformation.OutputWriter.DisposeAsync();
        }

        // The batch is in the local file now, so nothing below may throw, the batch would be emitted again otherwise.
        await this.UploadCompletedFiles(now);
    }

    /// <summary>
    /// Uploads the local files of the rolling intervals that are over, also while no events arrive.
    /// </summary>
    /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
    public Task OnEmptyBatchAsync() => this.UploadCompletedFiles(this.GetNow());

    /// <inheritdoc cref="IDisposable"/>
    /// <seealso cref="IDisposable"/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Uploads the local files that are left and releases the Amazon S3 client if the sink created it itself.
    /// </summary>
    /// <param name="disposing">A value indicating whether the managed resources should be released or not.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || this.disposed)
        {
            return;
        }

        this.disposed = true;

        // Task.Run keeps the upload away from a synchronization context of the caller, waiting for it could deadlock otherwise.
        Task.Run(this.UploadOnDispose).GetAwaiter().GetResult();

        if (this.ownsAmazonS3Client)
        {
            this.amazonS3Options.AmazonS3Client?.Dispose();
            this.amazonS3Options.AmazonS3Client = null;
            this.ownsAmazonS3Client = false;
        }
    }

    /// <summary>
    /// Gets the current local time from the time provider, the rolling intervals are based on it.
    /// </summary>
    /// <returns>The current local time as <see cref="DateTime"/>.</returns>
    private DateTime GetNow() => this.timeProvider.GetLocalNow().DateTime;

    /// <summary>
    /// Gets the path roller.
    /// </summary>
    /// <returns>The <see cref="PathRoller"/>.</returns>
    private PathRoller GetPathRoller() =>
        this.amazonS3Options.PathRoller ?? throw new InvalidOperationException("The path roller wasn't properly set.");

    /// <summary>
    /// Opens the given local file for appending and returns the file name and writer.
    /// </summary>
    /// <param name="fileName">The local file name.</param>
    /// <returns>The <see cref="FileInformation"/>.</returns>
    private FileInformation OpenFile(string fileName)
    {
        var directory = Path.GetDirectoryName(fileName);

        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var outputStream = File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);

        return new FileInformation
        {
            FileName = fileName,
            OutputWriter = new StreamWriter(outputStream, this.amazonS3Options.Encoding)
        };
    }

    /// <summary>
    /// Gets the local file for the events of the current rolling interval. A local file of the interval that is
    /// already there is continued, for example after a restart. Otherwise the first name that is not taken in the
    /// bucket yet is used, so that a restart without the local file does not overwrite the object uploaded before.
    /// </summary>
    /// <param name="now">The current date.</param>
    /// <returns>The local file name as <see cref="string"/>.</returns>
    private async Task<string> GetCurrentFileName(DateTime now)
    {
        if (this.currentFileName is not null
            && (this.amazonS3Options.NextCheckpoint is null || now < this.amazonS3Options.NextCheckpoint.Value)
            && File.Exists(this.currentFileName))
        {
            return this.currentFileName;
        }

        var pathRoller = this.GetPathRoller();
        var currentCheckpoint = pathRoller.GetCurrentCheckpoint(now);

        var localFile = this.GetLocalFiles()
            .Where(file => file.DateTime == currentCheckpoint)
            .OrderByDescending(file => file.SequenceNumber)
            .FirstOrDefault();

        var sequence = localFile is null ? await this.GetFirstSequenceNotInBucket(now) : localFile.SequenceNumber;
        this.currentFileName = this.GetLocalFileName(now, sequence);
        this.amazonS3Options.CurrentFileSequence = sequence;
        this.amazonS3Options.NextCheckpoint = pathRoller.GetNextCheckpoint(now);
        return this.currentFileName;
    }

    /// <summary>
    /// Gets the local file name for the rolling interval of the given date and the given sequence number.
    /// </summary>
    /// <param name="now">The current date.</param>
    /// <param name="sequence">The sequence number or <c>null</c> for none.</param>
    /// <returns>The local file name as <see cref="string"/>.</returns>
    private string GetLocalFileName(DateTime now, int? sequence)
    {
        this.GetPathRoller().GetLogFilePath(now, sequence, out var fileName);
        return fileName;
    }

    /// <summary>
    /// Gets the local files that match the configured path.
    /// </summary>
    /// <returns>The matching files as <see cref="RollingLogFile"/>s.</returns>
    private List<RollingLogFile> GetLocalFiles()
    {
        var pathRoller = this.GetPathRoller();

        if (!Directory.Exists(pathRoller.LogFileDirectory))
        {
            return [];
        }

        var fileNames = Directory.GetFiles(pathRoller.LogFileDirectory, pathRoller.DirectorySearchPattern)
            .Select(fileName => Path.GetFileName(fileName));
        return [.. pathRoller.SelectMatches(fileNames)];
    }

    /// <summary>
    /// Gets the first sequence number for the rolling interval of the given date whose key is not taken in the bucket.
    /// </summary>
    /// <param name="now">The current date.</param>
    /// <returns>The sequence number or <c>null</c> if the plain name is free.</returns>
    private async Task<int?> GetFirstSequenceNotInBucket(DateTime now)
    {
        int? sequence = null;

        while (await this.ExistsInBucket(this.GetLocalFileName(now, sequence)))
        {
            sequence = (sequence ?? 0) + 1;
        }

        return sequence;
    }

    /// <summary>
    /// Checks whether the key of the given local file is taken in the bucket.
    /// </summary>
    /// <param name="fileName">The local file name.</param>
    /// <returns>A value indicating whether the key is taken.</returns>
    private async Task<bool> ExistsInBucket(string fileName)
    {
        var request = new GetObjectMetadataRequest
        {
            BucketName = this.amazonS3Options.BucketName,
            Key = this.GetKey(fileName)
        };

        try
        {
            _ = await this.GetOrCreateClient().GetObjectMetadataAsync(request);
            return true;
        }
        catch (AmazonS3Exception amazonS3Exception)
            when (amazonS3Exception.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
        {
            // Without the s3:ListBucket permission Amazon S3 answers a missing key with Forbidden instead of NotFound.
            return false;
        }
    }

    /// <summary>
    /// Uploads and deletes the local files of the rolling intervals that are over. A file that fails to upload stays
    /// and is tried again with the next batch or batching period, the error goes to the <see cref="SelfLog"/>.
    /// </summary>
    /// <param name="now">The current date.</param>
    /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
    private async Task UploadCompletedFiles(DateTime now)
    {
        if (this.nextUploadCheck is not null && now < this.nextUploadCheck.Value)
        {
            return;
        }

        var allUploaded = true;

        try
        {
            var pathRoller = this.GetPathRoller();
            var currentCheckpoint = pathRoller.GetCurrentCheckpoint(now);

            var completedFiles = this.GetLocalFiles()
                .Where(file => file.DateTime < currentCheckpoint)
                .OrderBy(file => file.DateTime)
                .ThenBy(file => file.SequenceNumber);

            foreach (var file in completedFiles)
            {
                var fileName = Path.Combine(pathRoller.LogFileDirectory, file.FileName);
                allUploaded &= await this.TryUploadFile(fileName, true);
            }

            // No further interval can be over before the next checkpoint, RollingInterval.Infinite has none at all.
            this.nextUploadCheck = allUploaded ? pathRoller.GetNextCheckpoint(now) ?? DateTime.MaxValue : null;
        }
        catch (Exception exception)
        {
            SelfLog.WriteLine("Failed to check the local log files for completed rolling intervals: {0}", exception);
            this.nextUploadCheck = null;
        }
    }

    /// <summary>
    /// Uploads the local files of the rolling intervals that are over and the file of the current one when the sink is
    /// disposed. The file of the current interval is kept: a restart within the same interval continues it, and the
    /// upload at the end of the interval replaces the object with the complete file.
    /// </summary>
    /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
    private async Task UploadOnDispose()
    {
        var now = this.GetNow();
        this.nextUploadCheck = null;
        await this.UploadCompletedFiles(now);

        if (this.currentFileName is not null && File.Exists(this.currentFileName))
        {
            _ = await this.TryUploadFile(this.currentFileName, false);
        }
    }

    /// <summary>
    /// Uploads the given local file and deletes it afterwards if requested. An empty file is not uploaded, Amazon S3
    /// would store an empty object. Errors are written to the <see cref="SelfLog"/>.
    /// </summary>
    /// <param name="fileName">The local file name.</param>
    /// <param name="delete">A value indicating whether the file is deleted after the upload.</param>
    /// <returns>A value indicating whether the upload succeeded.</returns>
    private async Task<bool> TryUploadFile(string fileName, bool delete)
    {
        try
        {
            if (new FileInfo(fileName).Length > 0)
            {
                _ = await this.UploadFileToS3(fileName);
            }

            if (delete)
            {
                File.Delete(fileName);
            }

            return true;
        }
        catch (Exception exception)
        {
            SelfLog.WriteLine("Failed to upload the log file {0} to Amazon S3, it is kept and uploaded later: {1}", fileName, exception);
            return false;
        }
    }

    /// <summary>
    /// Gets the Amazon S3 client and creates it from the options if it wasn't given or created before.
    /// </summary>
    /// <returns>The <see cref="AmazonS3Client"/> to upload with.</returns>
    private AmazonS3Client GetOrCreateClient()
    {
        if (this.amazonS3Options.AmazonS3Client is not null)
        {
            return this.amazonS3Options.AmazonS3Client;
        }

        var config = new AmazonS3Config();

        if (this.amazonS3Options.Endpoint is not null)
        {
            config.RegionEndpoint = this.amazonS3Options.Endpoint;
        }
        else
        {
            config.ServiceURL = this.amazonS3Options.ServiceUrl;
        }

        // Since version 4 the AWS SDK adds a checksum to every request by default. Providers that need the payload
        // signing switched off usually don't support those checksums either, so the same option covers both.
        if (this.amazonS3Options.DisablePayloadSigning ?? false)
        {
            config.RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED;
        }

        // In the case that awsAccessKeyId and awsSecretAccessKey is passed, we use it. Otherwise authorization is given by roles in AWS directly.
        var client = !string.IsNullOrEmpty(this.amazonS3Options.AwsAccessKeyId) && !string.IsNullOrEmpty(this.amazonS3Options.AwsSecretAccessKey)
            ? new AmazonS3Client(this.amazonS3Options.AwsAccessKeyId, this.amazonS3Options.AwsSecretAccessKey, config)
            : new AmazonS3Client(config);

        // The client is kept for the lifetime of the sink, creating one per batch would open a new connection pool every time.
        this.amazonS3Options.AmazonS3Client = client;
        this.ownsAmazonS3Client = true;
        return client;
    }

    /// <summary>
    /// Gets the Amazon S3 key for the given local file: the bucket path plus the file name, the local directory never
    /// reaches Amazon S3.
    /// </summary>
    /// <param name="fileName">The local file name.</param>
    /// <returns>The key as <see cref="string"/>.</returns>
    private string GetKey(string fileName) =>
        string.IsNullOrWhiteSpace(this.amazonS3Options.BucketPath)
            ? Path.GetFileName(fileName).Replace("\\", "/")
            : Path.Combine(this.amazonS3Options.BucketPath, Path.GetFileName(fileName)).Replace("\\", "/");

    /// <summary>
    /// Uploads the file to a specified Amazon S3 bucket.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when an Unauthorized Access error
    /// condition occurs.
    /// </exception>
    /// <exception cref="AmazonS3Exception">
    /// Thrown when an Amazon S3 error condition
    /// occurs.
    /// </exception>
    /// <exception cref="Exception">
    /// Check the provided AWS credentials.
    /// </exception>
    /// <returns>
    /// An asynchronous result that yields a PutObjectResponse.
    /// </returns>
    private async Task<PutObjectResponse?> UploadFileToS3(string fileName)
    {
        var client = this.GetOrCreateClient();

        try
        {
            // S3 does not support updates, files are automatically rewritten. So we will have to upload the entire file.
            // Open the file for shared reading and writing.
            using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

            var putRequest = new PutObjectRequest
            {
                BucketName = this.amazonS3Options.BucketName,
                Key = this.GetKey(fileName),
                InputStream = fs,
                DisablePayloadSigning = this.amazonS3Options.DisablePayloadSigning
            };

            return await client.PutObjectAsync(putRequest);
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
            if (amazonS3Exception.ErrorCode != null && (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") || amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
            {
                throw new UnauthorizedAccessException("Check the provided AWS credentials.");
            }

            throw;
        }
    }
}
