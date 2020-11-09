// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AmazonS3Sink.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
// This class is the main class and contains all options for the AmazonS3 sink.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Amazon;
    using Amazon.S3;
    using Amazon.S3.Model;

    using Newtonsoft.Json;

    using Serilog.Debugging;
    using Serilog.Events;
    using Serilog.Formatting;
    using Serilog.Formatting.Display;
    using Serilog.Sinks.PeriodicBatching;

    /// <summary>
    /// This class is the main class and contains all options for the AmazonS3 sink.
    /// </summary>
    public class AmazonS3Sink : IBatchedLogEventSink
    {
        /// <summary>
        /// The AWS access key identifier.
        /// </summary>
        private readonly string awsAccessKeyId;

        /// <summary>
        /// The AWS secret access key.
        /// </summary>
        private readonly string awsSecretAccessKey;

        /// <summary>
        /// The Amazon S3 bucket name.
        /// </summary>
        private readonly string bucketName;

        /// <summary>
        /// The path where local files are stored.
        /// </summary>
        private readonly string bucketPath;

        /// <summary>
        /// The encoding.
        /// </summary>
        private readonly Encoding encoding;

        /// <summary>
        /// The Amazon S3 key endpoint.
        /// </summary>
        private readonly RegionEndpoint endpoint;

        /// <summary>
        /// The local path where the files are stored.
        /// </summary>
        private readonly string path;

        /// <summary>
        /// The Amazon S3 service url.
        /// </summary>
        private readonly string serviceUrl = "s3.amazonaws.com";

        /// <summary>
        /// The path roller.
        /// </summary>
        private readonly PathRoller pathRoller;

        /// <summary>
        /// The text formatter.
        /// </summary>
        private readonly ITextFormatter formatter;

        /// <summary>
        /// The next checkpoint.
        /// </summary>
        private DateTime? nextCheckpoint;

        /// <summary>
        /// The current file sequence number.
        /// </summary>
        private int? currentFileSequence;

        /// <summary>Initializes a new instance of the <see cref="AmazonS3Sink" /> class. </summary>
        /// <exception cref="ArgumentNullException">A given value is null.</exception>
        /// <param name="path">The path.</param>
        /// <param name="bucketName"> The Amazon S3 bucket name.</param>
        /// <param name="endpoint">The Amazon S3 endpoint.</param>
        /// <param name="awsAccessKeyId">The Amazon S3 access key id.</param>
        /// <param name="awsSecretAccessKey">The Amazon S3 secret access key.</param>
        /// <param name="outputTemplate">The output template.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="rollingInterval">The rolling interval.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="failureCallback">The failure callback.</param>
        /// <param name="bucketPath">The Amazon S3 bucket path.</param>
        public AmazonS3Sink(
            string path,
            string bucketName,
            RegionEndpoint endpoint,
            string awsAccessKeyId,
            string awsSecretAccessKey,
            string outputTemplate,
            IFormatProvider formatProvider,
            RollingInterval rollingInterval,
            Encoding encoding,
            Action<Exception> failureCallback = null,
            string bucketPath = null)
        {
            this.path = path;
            this.bucketName = bucketName;
            this.endpoint = endpoint;
            this.awsAccessKeyId = awsAccessKeyId;
            this.awsSecretAccessKey = awsSecretAccessKey;
            var textFormatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            this.formatter = textFormatter;
            this.pathRoller = new PathRoller(path, rollingInterval);
            this.encoding = encoding;
            this.FailureCallback = failureCallback;
            this.bucketPath = bucketPath;
        }

        /// <summary>Initializes a new instance of the <see cref="AmazonS3Sink" /> class. </summary>
        /// <exception cref="ArgumentNullException">A given value is null.</exception>
        /// <param name="path">The path.</param>
        /// <param name="bucketName"> The Amazon S3 bucket name.</param>
        /// <param name="endpoint">The Amazon S3 endpoint.</param>
        /// <param name="awsAccessKeyId">The Amazon S3 access key id.</param>
        /// <param name="awsSecretAccessKey">The Amazon S3 secret access key.</param>
        /// <param name="formatter">The formatter.</param>
        /// <param name="rollingInterval">The rolling interval.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="failureCallback">The failure callback.</param>
        /// <param name="bucketPath">The Amazon S3 bucket path.</param>
        public AmazonS3Sink(
            string path,
            string bucketName,
            RegionEndpoint endpoint,
            string awsAccessKeyId,
            string awsSecretAccessKey,
            ITextFormatter formatter,
            RollingInterval rollingInterval,
            Encoding encoding,
            Action<Exception> failureCallback = null,
            string bucketPath = null)
        {
            this.path = path;
            this.bucketName = bucketName;
            this.endpoint = endpoint;
            this.awsAccessKeyId = awsAccessKeyId;
            this.awsSecretAccessKey = awsSecretAccessKey;
            this.formatter = formatter;
            this.pathRoller = new PathRoller(path, rollingInterval);
            this.encoding = encoding;
            this.FailureCallback = failureCallback;
            this.bucketPath = bucketPath;
        }

        /// <summary>Initializes a new instance of the <see cref="AmazonS3Sink" /> class. </summary>
        /// <exception cref="ArgumentNullException">A given value is null.</exception>
        /// <param name="path">The path.</param>
        /// <param name="bucketName">The Amazon S3 bucket name.</param>
        /// <param name="endpoint">The Amazon S3 endpoint.</param>
        /// <param name="outputTemplate">The output template.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="rollingInterval">The rolling interval.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="failureCallback">The failure callback.</param>
        /// <param name="bucketPath">The Amazon S3 bucket path.</param>
        public AmazonS3Sink(
            string path,
            string bucketName,
            RegionEndpoint endpoint,
            string outputTemplate,
            IFormatProvider formatProvider,
            RollingInterval rollingInterval,
            Encoding encoding,
            Action<Exception> failureCallback = null,
            string bucketPath = null)
        {
            this.path = path;
            this.bucketName = bucketName;
            this.endpoint = endpoint;
            var textFormatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            this.formatter = textFormatter;
            this.pathRoller = new PathRoller(this.path, rollingInterval);
            this.encoding = encoding;
            this.FailureCallback = failureCallback;
            this.bucketPath = bucketPath;
        }

        /// <summary>Initializes a new instance of the <see cref="AmazonS3Sink" /> class. </summary>
        /// <exception cref="ArgumentNullException">A given value is null.</exception>
        /// <param name="path">The path.</param>
        /// <param name="bucketName">The Amazon S3 bucket name.</param>
        /// <param name="endpoint">The Amazon S3 endpoint.</param>
        /// <param name="formatter">The formatter.</param>
        /// <param name="rollingInterval">The rolling interval.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="failureCallback">The failure callback.</param>
        /// <param name="bucketPath">The Amazon S3 bucket path.</param>
        public AmazonS3Sink(
            string path,
            string bucketName,
            RegionEndpoint endpoint,
            ITextFormatter formatter,
            RollingInterval rollingInterval,
            Encoding encoding,
            Action<Exception> failureCallback = null,
            string bucketPath = null)
        {
            this.path = path;
            this.bucketName = bucketName;
            this.endpoint = endpoint;
            this.formatter = formatter;
            this.pathRoller = new PathRoller(this.path, rollingInterval);
            this.encoding = encoding;
            this.FailureCallback = failureCallback;
            this.bucketPath = bucketPath;
        }

        /// <summary>Initializes a new instance of the <see cref="AmazonS3Sink" /> class. </summary>
        /// <exception cref="ArgumentNullException">A given value is null.</exception>
        /// <param name="path">The path.</param>
        /// <param name="bucketName"> The Amazon S3 bucket name.</param>
        /// <param name="serviceUrl">The Amazon S3 service url.</param>
        /// <param name="awsAccessKeyId">The Amazon S3 access key id.</param>
        /// <param name="awsSecretAccessKey">The Amazon S3 secret access key.</param>
        /// <param name="outputTemplate">The output template.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="rollingInterval">The rolling interval.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="failureCallback">The failure callback.</param>
        /// <param name="bucketPath">The Amazon S3 bucket path.</param>
        public AmazonS3Sink(
            string path,
            string bucketName,
            string serviceUrl,
            string awsAccessKeyId,
            string awsSecretAccessKey,
            string outputTemplate,
            IFormatProvider formatProvider,
            RollingInterval rollingInterval,
            Encoding encoding,
            Action<Exception> failureCallback = null,
            string bucketPath = null)
        {
            this.path = path;
            this.bucketName = bucketName;
            this.serviceUrl = serviceUrl;
            this.awsAccessKeyId = awsAccessKeyId;
            this.awsSecretAccessKey = awsSecretAccessKey;
            var textFormatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            this.formatter = textFormatter;
            this.pathRoller = new PathRoller(path, rollingInterval);
            this.encoding = encoding;
            this.FailureCallback = failureCallback;
            this.bucketPath = bucketPath;
        }

        /// <summary>Initializes a new instance of the <see cref="AmazonS3Sink" /> class. </summary>
        /// <exception cref="ArgumentNullException">A given value is null.</exception>
        /// <param name="path">The path.</param>
        /// <param name="bucketName"> The Amazon S3 bucket name.</param>
        /// <param name="serviceUrl">The Amazon S3 service url.</param>
        /// <param name="awsAccessKeyId">The Amazon S3 access key id.</param>
        /// <param name="awsSecretAccessKey">The Amazon S3 secret access key.</param>
        /// <param name="formatter">The formatter.</param>
        /// <param name="rollingInterval">The rolling interval.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="failureCallback">The failure callback.</param>
        /// <param name="bucketPath">The Amazon S3 bucket path.</param>
        public AmazonS3Sink(
            string path,
            string bucketName,
            string serviceUrl,
            string awsAccessKeyId,
            string awsSecretAccessKey,
            ITextFormatter formatter,
            RollingInterval rollingInterval,
            Encoding encoding,
            Action<Exception> failureCallback = null,
            string bucketPath = null)
        {
            this.path = path;
            this.bucketName = bucketName;
            this.serviceUrl = serviceUrl;
            this.awsAccessKeyId = awsAccessKeyId;
            this.awsSecretAccessKey = awsSecretAccessKey;
            this.formatter = formatter;
            this.pathRoller = new PathRoller(path, rollingInterval);
            this.encoding = encoding;
            this.FailureCallback = failureCallback;
            this.bucketPath = bucketPath;
        }

        /// <summary>Initializes a new instance of the <see cref="AmazonS3Sink" /> class. </summary>
        /// <exception cref="ArgumentNullException">A given value is null.</exception>
        /// <param name="path">The path.</param>
        /// <param name="bucketName">The Amazon S3 bucket name.</param>
        /// <param name="serviceUrl">The Amazon S3 service url.</param>
        /// <param name="outputTemplate">The output template.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="rollingInterval">The rolling interval.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="failureCallback">The failure callback.</param>
        /// <param name="bucketPath">The Amazon S3 bucket path.</param>
        public AmazonS3Sink(
            string path,
            string bucketName,
            string serviceUrl,
            string outputTemplate,
            IFormatProvider formatProvider,
            RollingInterval rollingInterval,
            Encoding encoding,
            Action<Exception> failureCallback = null,
            string bucketPath = null)
        {
            this.path = path;
            this.bucketName = bucketName;
            this.serviceUrl = serviceUrl;
            var textFormatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            this.formatter = textFormatter;
            this.pathRoller = new PathRoller(this.path, rollingInterval);
            this.encoding = encoding;
            this.FailureCallback = failureCallback;
            this.bucketPath = bucketPath;
        }

        /// <summary>Initializes a new instance of the <see cref="AmazonS3Sink" /> class. </summary>
        /// <exception cref="ArgumentNullException">A given value is null.</exception>
        /// <param name="path">The path.</param>
        /// <param name="bucketName">The Amazon S3 bucket name.</param>
        /// <param name="serviceUrl">The Amazon S3 service url.</param>
        /// <param name="formatter">The formatter.</param>
        /// <param name="rollingInterval">The rolling interval.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="failureCallback">The failure callback.</param>
        /// <param name="bucketPath">The Amazon S3 bucket path.</param>
        public AmazonS3Sink(
            string path,
            string bucketName,
            string serviceUrl,
            ITextFormatter formatter,
            RollingInterval rollingInterval,
            Encoding encoding,
            Action<Exception> failureCallback = null,
            string bucketPath = null)
        {
            this.path = path;
            this.bucketName = bucketName;
            this.serviceUrl = serviceUrl;
            this.formatter = formatter;
            this.pathRoller = new PathRoller(this.path, rollingInterval);
            this.encoding = encoding;
            this.FailureCallback = failureCallback;
            this.bucketPath = bucketPath;
        }

        /// <summary>
        /// Gets or sets the failure callback.
        /// </summary>
        public Action<Exception> FailureCallback { get; set; }

        /// <summary>Emit a batch of log events, running asynchronously.</summary>
        /// <param name="batch">The batch of events to emit.</param>
        /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
        public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
        {
            if (batch == null)
            {
                throw new ArgumentNullException(nameof(batch));
            }

            var fileInformation = this.OpenFile();

            foreach (var logEvent in batch)
            {
                this.formatter.Format(logEvent, fileInformation.OutputWriter);
            }

            await fileInformation.OutputWriter.FlushAsync();
            fileInformation.OutputWriter.Close();

            try
            {
                var result = await this.UploadFileToS3(fileInformation.FileName);
                SelfLog.WriteLine($"Uploaded data to Amazon S3 with result: {JsonConvert.SerializeObject(result)}.");
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine($"{ex.Message}{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Allows sinks to perform periodic work without requiring additional threads
        /// or timers (thus avoiding additional flush/shut-down complexity).
        /// </summary>
        /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
        public async Task OnEmptyBatchAsync()
        {
            await Task.Delay(0);
        }

        /// <summary>
        /// Open a file and returns the file name and file stream.
        /// </summary>
        /// <returns>The <see cref="FileInformation"/>.</returns>
        private FileInformation OpenFile()
        {
            var fileName = this.AlignCurrentFileTo(DateTime.Now, true);

            var directory = Path.GetDirectoryName(this.path);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var outputStream = File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
            return new FileInformation
            {
                FileName = fileName,
                OutputWriter = new StreamWriter(outputStream, this.encoding)
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
            if (!this.nextCheckpoint.HasValue)
            {
                return this.GetFileName(now);
            }

            // ReSharper disable once InvertIf
            if (nextSequence || now >= this.nextCheckpoint.Value)
            {
                int? minSequence = null;

                // ReSharper disable once InvertIf
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

                return this.GetFileName(now, minSequence);
            }

            return null;
        }

        /// <summary>
        /// Gets the file name according to the rolling file sequence.
        /// </summary>
        /// <param name="now">The current date.</param>
        /// <param name="minSequence">The minimum sequence number.</param>
        /// <returns>The file name as <see cref="string"/>.</returns>
        private string GetFileName(DateTime now, int? minSequence = null)
        {
            var currentCheckpoint = this.pathRoller.GetCurrentCheckpoint(now);
            this.nextCheckpoint = this.pathRoller.GetNextCheckpoint(now);

            var existingFiles = Enumerable.Empty<string>();

            try
            {
                if (Directory.Exists(this.pathRoller.LogFileDirectory))
                {
                    existingFiles = Directory.GetFiles(this.pathRoller.LogFileDirectory, this.pathRoller.DirectorySearchPattern).Select(Path.GetFileName);
                }
            }
            catch (DirectoryNotFoundException)
            {
                // Ignored
            }

            var latestForThisCheckpoint = this.pathRoller
                .SelectMatches(existingFiles)
                .Where(m => m.DateTime == currentCheckpoint)
                .OrderByDescending(m => m.SequenceNumber)
                .FirstOrDefault();

            var sequence = latestForThisCheckpoint?.SequenceNumber;

            if (minSequence != null)
            {
                if (sequence == null || sequence.Value < minSequence.Value)
                {
                    sequence = minSequence;
                }
            }

            this.pathRoller.GetLogFilePath(now, sequence, out var localPath);
            this.currentFileSequence = sequence;
            return localPath;
        }

        /// <summary>
        /// Uploads the file to a specified Amazon S3 bucket. 
        /// </summary>
        /// <param name="fileName"> he file name.</param>
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
        // ReSharper disable once UnusedMethodReturnValue.Local
        private async Task<PutObjectResponse> UploadFileToS3(string fileName)
        {
            AmazonS3Client client;

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (this.endpoint != null)
            {
                client = new AmazonS3Client(this.endpoint);
            }
            else
            {
                client = new AmazonS3Client(
                    new AmazonS3Config
                    {
                        ServiceURL = this.serviceUrl
                    });
            }

            // In the case that awsAccessKeyId and awsSecretAccessKey is passed, we use it. Otherwise authorization is given by roles in AWS directly.
            if (!string.IsNullOrEmpty(this.awsAccessKeyId) && !string.IsNullOrEmpty(this.awsSecretAccessKey))
            {
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (this.endpoint != null)
                {
                    client = new AmazonS3Client(this.awsAccessKeyId, this.awsSecretAccessKey, this.endpoint);
                }
                else
                {
                    client = new AmazonS3Client(
                        this.awsAccessKeyId,
                        this.awsSecretAccessKey,
                        new AmazonS3Config
                        {
                            ServiceURL = this.serviceUrl
                        });
                }
            }

            try
            {
                // S3 does not support updates, files are automatically rewritten so we will have to upload the entire file
                // Open the file for shared reading and writing
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var key = string.IsNullOrWhiteSpace(this.bucketPath) ? Path.GetFileName(fileName).Replace("\\", "/") : Path.Combine(this.bucketPath, Path.GetFileName(fileName)).Replace("\\", "/");

                    if (fs.Length == 0)
                    {
                        return null;
                    }

                    var putRequest = new PutObjectRequest
                    {
                        BucketName = this.bucketName,
                        Key = key,
                        InputStream = fs
                    };

                    return await client.PutObjectAsync(putRequest);
                }
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
}
