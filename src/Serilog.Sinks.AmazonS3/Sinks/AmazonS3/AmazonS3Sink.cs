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
/// </summary>
public class AmazonS3Sink : IBatchedLogEventSink, IDisposable
{
    /// <summary>
    /// The Amazon S3 options.
    /// </summary>
    private readonly AmazonS3Options amazonS3Options = new();

    /// <summary>
    /// A value indicating whether the sink created the Amazon S3 client itself and therefore needs to dispose it.
    /// </summary>
    private bool ownsAmazonS3Client;

    /// <summary>Initializes a new instance of the <see cref="AmazonS3Sink" /> class. </summary>
    /// <exception cref="ArgumentNullException">A given value is null.</exception>
    /// <param name="amazonS3Options">The Amazon S3 options.</param>
    public AmazonS3Sink(AmazonS3Options amazonS3Options)
    {
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

    /// <summary>Emit a batch of log events, running asynchronously.</summary>
    /// <param name="batch">The batch of events to emit.</param>
    /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
    public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
    {
        var fileInformation = this.OpenFile();

        if (fileInformation.OutputWriter is null)
        {
            throw new InvalidOperationException($"The output writer was not properly set for {fileInformation.FileName}");
        }

        try
        {
            foreach (var logEvent in batch)
            {
                this.amazonS3Options.Formatter?.Format(logEvent, fileInformation.OutputWriter);
            }

            await fileInformation.OutputWriter.FlushAsync();
        }
        finally
        {
            // The writer needs to be closed in any case, otherwise a formatter that throws leaves the file locked.
            await fileInformation.OutputWriter.DisposeAsync();
        }

        _ = await this.UploadFileToS3(fileInformation.FileName);
        File.Delete(fileInformation.FileName);
    }

    /// <summary>
    /// Allows sinks to perform periodic work without requiring additional threads
    /// or timers (thus avoiding additional flush/shut-down complexity).
    /// </summary>
    /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
    public Task OnEmptyBatchAsync() => Task.CompletedTask;

    /// <inheritdoc cref="IDisposable"/>
    /// <seealso cref="IDisposable"/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the Amazon S3 client if the sink created it itself.
    /// </summary>
    /// <param name="disposing">A value indicating whether the managed resources should be released or not.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || !this.ownsAmazonS3Client)
        {
            return;
        }

        this.amazonS3Options.AmazonS3Client?.Dispose();
        this.amazonS3Options.AmazonS3Client = null;
        this.ownsAmazonS3Client = false;
    }

    /// <summary>
    /// Open a file and returns the file name and file stream.
    /// </summary>
    /// <returns>The <see cref="FileInformation"/>.</returns>
    private FileInformation OpenFile()
    {
        var fileName = this.AlignCurrentFileTo(DateTime.Now, true);

        var directory = Path.GetDirectoryName(this.amazonS3Options.Path);

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
    /// Aligns the current file name to the sequence and returns the file name.
    /// </summary>
    /// <param name="now">The current date.</param>
    /// <param name="nextSequence">The next sequence number.</param>
    /// <returns>The file name as <see cref="string"/>.</returns>
    private string AlignCurrentFileTo(DateTime now, bool nextSequence = false)
    {
        if (!this.amazonS3Options.NextCheckpoint.HasValue)
        {
            return this.GetFileName(now);
        }

        if (nextSequence || now >= this.amazonS3Options.NextCheckpoint.Value)
        {
            int? minSequence = null;

            if (nextSequence)
            {
                if (this.amazonS3Options.CurrentFileSequence is null)
                {
                    minSequence = 1;
                }
                else
                {
                    minSequence = this.amazonS3Options.CurrentFileSequence.Value + 1;
                }
            }

            return this.GetFileName(now, minSequence);
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets the file name according to the rolling file sequence.
    /// </summary>
    /// <param name="now">The current date.</param>
    /// <param name="minSequence">The minimum sequence number.</param>
    /// <returns>The file name as <see cref="string"/>.</returns>
    private string GetFileName(DateTime now, int? minSequence = null)
    {
        if (this.amazonS3Options.PathRoller is null)
        {
            throw new InvalidOperationException("The path roller wasn't properly set.");
        }

        var currentCheckpoint = this.amazonS3Options.PathRoller.GetCurrentCheckpoint(now);
        this.amazonS3Options.NextCheckpoint = this.amazonS3Options.PathRoller.GetNextCheckpoint(now);

        var existingFiles = new List<string>();

        try
        {
            if (Directory.Exists(this.amazonS3Options.PathRoller.LogFileDirectory))
            {
                var directoryFiles = Directory.GetFiles(this.amazonS3Options.PathRoller.LogFileDirectory, this.amazonS3Options.PathRoller.DirectorySearchPattern);
                var files = directoryFiles.Select(f => Path.GetFileName(f) ?? string.Empty)?.ToList() ?? new List<string>();
                existingFiles = files ?? new List<string>();
            }
        }
        catch (DirectoryNotFoundException)
        {
            // Ignored
        }

        var latestForThisCheckpoint = this.amazonS3Options.PathRoller
            .SelectMatches(existingFiles)
            .Where(m => m.DateTime == currentCheckpoint)
            .OrderByDescending(m => m.SequenceNumber)
            .FirstOrDefault();

        var sequence = latestForThisCheckpoint?.SequenceNumber;

        if (minSequence is not null)
        {
            if (sequence is null || sequence.Value < minSequence.Value)
            {
                sequence = minSequence;
            }
        }

        this.amazonS3Options.PathRoller.GetLogFilePath(now, sequence, out var localPath);
        this.amazonS3Options.CurrentFileSequence = sequence;
        return localPath;
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

            var key = string.IsNullOrWhiteSpace(this.amazonS3Options.BucketPath)
                          ? Path.GetFileName(fileName).Replace("\\", "/")
                          : Path.Combine(this.amazonS3Options.BucketPath, Path.GetFileName(fileName))
                              .Replace("\\", "/");

            if (fs.Length is 0)
            {
                throw new InvalidOperationException("The file size is 0.");
            }

            var putRequest = new PutObjectRequest
            {
                BucketName = this.amazonS3Options.BucketName,
                Key = key,
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
