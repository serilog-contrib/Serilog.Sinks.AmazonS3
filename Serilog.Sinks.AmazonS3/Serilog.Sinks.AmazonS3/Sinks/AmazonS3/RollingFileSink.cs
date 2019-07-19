// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RollingFileSink.cs" company="Hämmer Electronics">
// The project is licensed under the GNU GENERAL PUBLIC LICENSE, Version 3, 29 June 2007
// </copyright>
// <summary>
//   Defines the RollingFileSink type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.S3;
    using Amazon.S3.Model;

    using Serilog.Core;
    using Serilog.Debugging;
    using Serilog.Events;
    using Serilog.Formatting;

    /// <summary>   A class to write rolling files. </summary>
    ///
    /// <seealso cref="ILogEventSink"/>
    /// <seealso cref="IFlushableFileSink"/>
    /// <seealso cref="IDisposable"/>
    ///
    /// ### <inheritdoc cref="IFlushableFileSink"/>
    /// <inheritdoc cref="IDisposable"/>

    public class RollingFileSink : ILogEventSink, IFlushableFileSink, IDisposable
    {
        /// <summary>   The aws access key identifier. </summary>
        private readonly string awsAccessKeyId;

        /// <summary>   The aws secret access key. </summary>
        private readonly string awsSecretAccessKey;

        /// <summary>   The Amazon S3 bucket name. </summary>
        private readonly string bucketName;

        /// <summary>   The buffered bool value. </summary>
        private readonly bool buffered;

        /// <summary>   The encoding. </summary>
        private readonly Encoding encoding;

        /// <summary>   The Amazon S3 key endpoint. </summary>
        private readonly RegionEndpoint endpoint;

        /// <summary>   The file lifecycle hooks. </summary>
        private readonly FileLifecycleHooks fileLifecycleHooks;

        /// <summary>   The file size limit bytes. </summary>
        private readonly long? fileSizeLimitBytes;

        /// <summary>   The path roller. </summary>
        private readonly PathRoller pathRoller;

        /// <summary>   The retained file count limit. </summary>
        private readonly int? retainedFileCountLimit;

        /// <summary>   The roll on file size limit bool value. </summary>
        private readonly bool rollOnFileSizeLimit;

        /// <summary>   The synchronize root. </summary>
        private readonly object syncRoot = new object();

        /// <summary>   The text formatter. </summary>
        private readonly ITextFormatter textFormatter;

        /// <summary>   The current file. </summary>
        private IFileSink currentFile;

        /// <summary>   The current file name. </summary>
        private string currentFileName;

        /// <summary>   The current file sequence. </summary>
        private int? currentFileSequence;

        /// <summary>   The is disposed bool value. </summary>
        private bool isDisposed;

        /// <summary>   The next checkpoint. </summary>
        private DateTime? nextCheckpoint;

        /// <summary>   Initializes a new instance of the <see cref="RollingFileSink" /> class. </summary>
        ///
        /// <exception cref="ArgumentNullException">    An <see cref="ArgumentNullException" /> thrown
        ///                                             when the path is null. </exception>
        /// <exception cref="ArgumentException">        Negative value provided; file size limit must be
        ///                                             non-negative. or Zero or negative value provided;
        ///                                             retained file count limit must be at least 1. </exception>
        ///
        /// <param name="path">                     The path. </param>
        /// <param name="textFormatter">            The text formatter. </param>
        /// <param name="fileSizeLimitBytes">       The file size limit bytes. </param>
        /// <param name="retainedFileCountLimit">   The retained file count limit. </param>
        /// <param name="encoding">                 The encoding. </param>
        /// <param name="buffered">                 if set to <c>true</c> [buffered]. </param>
        /// <param name="rollingInterval">          The rolling interval. </param>
        /// <param name="rollOnFileSizeLimit">      if set to <c>true</c> [roll on file size limit]. </param>
        /// <param name="fileLifecycleHooks">       The file lifecycle hooks. </param>
        /// <param name="bucketName">               The Amazon S3 bucket name. </param>
        /// <param name="endpoint">                 The Amazon S3 endpoint. </param>
        /// <param name="awsAccessKeyId">           The Amazon S3 access key id. </param>
        /// <param name="awsSecretAccessKey">       The Amazon S3 access key. </param>

        public RollingFileSink(
            string path,
            ITextFormatter textFormatter,
            long? fileSizeLimitBytes,
            int? retainedFileCountLimit,
            Encoding encoding,
            bool buffered,
            RollingInterval rollingInterval,
            bool rollOnFileSizeLimit,
            FileLifecycleHooks fileLifecycleHooks,
            string bucketName,
            RegionEndpoint endpoint,
            string awsAccessKeyId,
            string awsSecretAccessKey)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            if (string.IsNullOrWhiteSpace(awsAccessKeyId))
            {
                throw new ArgumentNullException(nameof(awsAccessKeyId));
            }

            if (string.IsNullOrWhiteSpace(awsSecretAccessKey))
            {
                throw new ArgumentNullException(nameof(awsSecretAccessKey));
            }

            if (fileSizeLimitBytes.HasValue && fileSizeLimitBytes < 0)
            {
                throw new ArgumentException("Negative value provided; file size limit must be non-negative.");
            }

            if (retainedFileCountLimit.HasValue && retainedFileCountLimit < 1)
            {
                throw new ArgumentException(
                    "Zero or negative value provided; retained file count limit must be at least 1.");
            }

            this.bucketName = bucketName;
            this.awsAccessKeyId = awsAccessKeyId;
            this.awsSecretAccessKey = awsSecretAccessKey;
            this.endpoint = endpoint;
            this.pathRoller = new PathRoller(path, rollingInterval);
            this.textFormatter = textFormatter;
            this.fileSizeLimitBytes = fileSizeLimitBytes;
            this.retainedFileCountLimit = retainedFileCountLimit;
            this.encoding = encoding;
            this.buffered = buffered;
            this.rollOnFileSizeLimit = rollOnFileSizeLimit;
            this.fileLifecycleHooks = fileLifecycleHooks;
        }

        /// <summary>   Initializes a new instance of the <see cref="RollingFileSink" /> class. </summary>
        ///
        /// <exception cref="ArgumentNullException">    An <see cref="ArgumentNullException" /> thrown
        ///                                             when the path is null. </exception>
        /// <exception cref="ArgumentException">        Negative value provided; file size limit must be
        ///                                             non-negative. or Zero or negative value provided;
        ///                                             retained file count limit must be at least 1. </exception>
        ///
        /// <param name="path">                     The path. </param>
        /// <param name="textFormatter">            The text formatter. </param>
        /// <param name="fileSizeLimitBytes">       The file size limit bytes. </param>
        /// <param name="retainedFileCountLimit">   The retained file count limit. </param>
        /// <param name="encoding">                 The encoding. </param>
        /// <param name="buffered">                 if set to <c>true</c> [buffered]. </param>
        /// <param name="rollingInterval">          The rolling interval. </param>
        /// <param name="rollOnFileSizeLimit">      if set to <c>true</c> [roll on file size limit]. </param>
        /// <param name="fileLifecycleHooks">       The file lifecycle hooks. </param>
        /// <param name="bucketName">               The Amazon S3 bucket name. </param>
        /// <param name="endpoint">                 The Amazon S3 endpoint. </param>

        public RollingFileSink(
            string path,
            ITextFormatter textFormatter,
            long? fileSizeLimitBytes,
            int? retainedFileCountLimit,
            Encoding encoding,
            bool buffered,
            RollingInterval rollingInterval,
            bool rollOnFileSizeLimit,
            FileLifecycleHooks fileLifecycleHooks,
            string bucketName,
            RegionEndpoint endpoint)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            if (fileSizeLimitBytes.HasValue && fileSizeLimitBytes < 0)
            {
                throw new ArgumentException("Negative value provided; file size limit must be non-negative.");
            }

            if (retainedFileCountLimit.HasValue && retainedFileCountLimit < 1)
            {
                throw new ArgumentException(
                    "Zero or negative value provided; retained file count limit must be at least 1.");
            }

            this.bucketName = bucketName;
            this.endpoint = endpoint;
            this.pathRoller = new PathRoller(path, rollingInterval);
            this.textFormatter = textFormatter;
            this.fileSizeLimitBytes = fileSizeLimitBytes;
            this.retainedFileCountLimit = retainedFileCountLimit;
            this.encoding = encoding;
            this.buffered = buffered;
            this.rollOnFileSizeLimit = rollOnFileSizeLimit;
            this.fileLifecycleHooks = fileLifecycleHooks;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting
        ///     unmanaged resources.
        /// </summary>
        ///
        /// <inheritdoc cref="IDisposable"/>

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting
        ///     unmanaged resources.
        /// </summary>
        ///
        /// <param name="disposing">    True to release both managed and unmanaged resources; false to
        ///                             release only unmanaged resources. </param>

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
                return;

            if (disposing)
            {
                lock (this.syncRoot)
                {
                    if (this.currentFile == null)
                    {
                        return;
                    }

                    this.CloseFile();
                    this.isDisposed = true;
                }
            }

            this.isDisposed = true;
        }

        /// <summary>
        ///     Use C# destructor syntax for finalization code. This destructor will run only if the
        ///     Dispose method does not get called. It gives your base class the opportunity to finalize.
        ///     Do not provide destructors in types derived from this class.
        /// </summary>

        ~RollingFileSink()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        /// <summary>   Emit the provided log event to the sink. </summary>
        ///
        /// <exception cref="ArgumentNullException">    logEvent. </exception>
        /// <exception cref="ObjectDisposedException">  The log file has been disposed. </exception>
        ///
        /// <param name="logEvent"> The log event to write. </param>
        ///
        /// <inheritdoc cref="ILogEventSink"/>

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }

            lock (this.syncRoot)
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException("The log file has been disposed.");
                }

                var now = DateTime.Now;
                this.AlignCurrentFileTo(now);

                while (this.currentFile?.EmitOrOverflow(logEvent) == false && this.rollOnFileSizeLimit)
                {
                    this.AlignCurrentFileTo(now, true);
                }
            }
        }

        /// <summary>   Flush buffered contents to the disk. </summary>
        ///
        /// <inheritdoc cref="IFlushableFileSink"/>

        public void FlushToDisk()
        {
            lock (this.syncRoot)
            {
                this.currentFile?.FlushToDisk();
            }
        }

        /// <summary>   Aligns the current file to the current <see cref="DateTime" />. </summary>
        ///
        /// <param name="now">          The current <see cref="DateTime" />. </param>
        /// <param name="nextSequence"> (Optional) Uses the next sequence if set to <c>true</c>. </param>

        private void AlignCurrentFileTo(DateTime now, bool nextSequence = false)
        {
            if (!this.nextCheckpoint.HasValue)
            {
                this.OpenFile(now);
            }
            else if (nextSequence || now >= this.nextCheckpoint.Value)
            {
                int? minSequence = null;
                if (nextSequence)
                {
                    if (this.currentFileSequence == null)
                    {
                        minSequence = 1;
                    }
                    else
                    {
                        minSequence = this.currentFileSequence.Value + 1;
                    }
                }

                this.CloseFile();

                this.UploadFileToS3().Wait();

                this.OpenFile(now, minSequence);
            }
        }

        /// <summary>   Uploads the file to a specified Amazon S3 bucket. </summary>
        ///
        /// <exception cref="Exception">    Check the provided AWS Credentials. or Error occurred: " +
        ///                                 amazonS3Exception.Message. </exception>

        private async Task<PutObjectResponse> UploadFileToS3()
        {
            AmazonS3Client client = new AmazonS3Client(this.endpoint);

            // In the case of AWSAccess is passed we use it, otherwize autorisation is given by roles in AWS directly
            if (!string.IsNullOrEmpty(this.awsAccessKeyId) && !string.IsNullOrEmpty(this.awsSecretAccessKey))
            {
                client = new AmazonS3Client(this.awsAccessKeyId, this.awsSecretAccessKey, this.endpoint);
            }

            try
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = this.bucketName,
                    Key = Path.GetFileName(this.currentFileName),
                    FilePath = this.currentFileName,
                    ContentType = "text/plain"
                };

                return await client.PutObjectAsync(putRequest);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null
                    && (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                        || amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new UnauthorizedAccessException("Check the provided AWS Credentials.");
                }

                throw;
            }
        }

        /// <summary>   Applies the retention policy. </summary>
        ///
        /// <param name="currentFilePath">  The current file path. </param>

        private void ApplyRetentionPolicy(string currentFilePath)
        {
            if (this.retainedFileCountLimit == null)
            {
                return;
            }

            var currentFileNameLocal = Path.GetFileName(currentFilePath);

            // We consider the current file to exist, even if nothing's been written yet,
            // because files are only opened on response to an event being processed.
            var potentialMatches = Directory
                .GetFiles(this.pathRoller.LogFileDirectory, this.pathRoller.DirectorySearchPattern)
                .Select(Path.GetFileName).Union(new[] { currentFileNameLocal });

            var newestFirst = this.pathRoller.SelectMatches(potentialMatches).OrderByDescending(m => m.DateTime)
                .ThenByDescending(m => m.SequenceNumber).Select(m => m.Filename);

            var toRemove = newestFirst.Where(n => StringComparer.OrdinalIgnoreCase.Compare(currentFileNameLocal, n) != 0)
                .Skip(this.retainedFileCountLimit.Value - 1).ToList();

            foreach (var obsolete in toRemove)
            {
                var fullPath = Path.Combine(this.pathRoller.LogFileDirectory, obsolete);
                try
                {
                    File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    SelfLog.WriteLine("Error {0} while removing obsolete log file {1}", ex, fullPath);
                    throw;
                }
            }
        }

        /// <summary>   Closes the file. </summary>
        private void CloseFile()
        {
            if (this.currentFile != null)
            {
                (this.currentFile as IDisposable)?.Dispose();
                this.currentFile = null;
            }

            this.nextCheckpoint = null;
        }

        /// <summary>   Opens the file. </summary>
        ///
        /// <exception cref="DirectoryNotFoundException">   Thrown when the requested directory is not
        ///                                                 present. </exception>
        /// <exception cref="IOException">                  Thrown when an IO failure occurred. </exception>
        ///
        /// <param name="now">          The now. </param>
        /// <param name="minSequence">  (Optional) The minimum sequence. </param>

        private void OpenFile(DateTime now, int? minSequence = null)
        {
            var currentCheckpoint = this.pathRoller.GetCurrentCheckpoint(now);

            // We only try periodically because repeated failures
            // to open log files REALLY slow an app down.
            this.nextCheckpoint = this.pathRoller.GetNextCheckpoint(now) ?? now.AddMinutes(30);

            var existingFiles = Enumerable.Empty<string>();
            try
            {
                if (Directory.Exists(this.pathRoller.LogFileDirectory))
                {
                    existingFiles = Directory.GetFiles(
                        this.pathRoller.LogFileDirectory,
                        this.pathRoller.DirectorySearchPattern).Select(Path.GetFileName);
                }
            }
            catch (DirectoryNotFoundException)
            {
                SelfLog.WriteLine("Temporary log directory is not found");
                throw;
            }

            var latestForThisCheckpoint = this.pathRoller.SelectMatches(existingFiles)
                .Where(m => m.DateTime == currentCheckpoint).OrderByDescending(m => m.SequenceNumber).FirstOrDefault();

            var sequence = latestForThisCheckpoint?.SequenceNumber;
            if (minSequence != null && (sequence == null || sequence.Value < minSequence.Value))
            {
                sequence = minSequence;
            }

            const int MaxAttempts = 3;
            for (var attempt = 0; attempt < MaxAttempts; attempt++)
            {
                this.pathRoller.GetLogFilePath(now, sequence, out var path);

                try
                {
                    this.currentFile = new FileSink(
                        path,
                        this.textFormatter,
                        this.fileSizeLimitBytes,
                        this.encoding,
                        this.buffered,
                        this.fileLifecycleHooks);

                    this.currentFileName = path;

                    this.currentFileSequence = sequence;
                }
                catch (IOException ex)
                {
                    if (!IoErrors.IsLockedFile(ex))
                    {
                        throw;
                    }

                    SelfLog.WriteLine(
                        "File target {0} was locked, attempting to open next in sequence (attempt {1})",
                        path,
                        attempt + 1);
                    sequence = (sequence ?? 0) + 1;
                    continue;
                }

                this.ApplyRetentionPolicy(path);
                return;
            }
        }
    }
}