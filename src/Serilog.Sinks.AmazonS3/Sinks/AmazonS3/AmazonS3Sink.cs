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
    using System.Threading.Tasks;

    using Amazon.S3;
    using Amazon.S3.Model;

    using Newtonsoft.Json;

    using Serilog.Debugging;
    using Serilog.Events;
    using Serilog.Formatting.Display;
    using Serilog.Sinks.PeriodicBatching;

    /// <summary>
    /// This class is the main class and contains all options for the AmazonS3 sink.
    /// </summary>
    public class AmazonS3Sink : IBatchedLogEventSink
    {
        /// <summary>
        /// The Amazon S3 options.
        /// </summary>
        private readonly AmazonS3Options amazonS3Options = new AmazonS3Options();

        /// <summary>Initializes a new instance of the <see cref="AmazonS3Sink" /> class. </summary>
        /// <exception cref="ArgumentNullException">A given value is null.</exception>
        /// <param name="amazonS3Options">The Amazon S3 options.</param>
        public AmazonS3Sink(AmazonS3Options amazonS3Options)
        {
            this.amazonS3Options.Path = amazonS3Options.Path;
            this.amazonS3Options.BucketName = amazonS3Options.BucketName;
            this.amazonS3Options.Endpoint = amazonS3Options.Endpoint;
            this.amazonS3Options.AwsAccessKeyId = amazonS3Options.AwsAccessKeyId;
            this.amazonS3Options.AwsSecretAccessKey = amazonS3Options.AwsSecretAccessKey;

            if (amazonS3Options.Formatter is null)
            {
                var textFormatter = new MessageTemplateTextFormatter(amazonS3Options.OutputTemplate, amazonS3Options.FormatProvider);
                this.amazonS3Options.Formatter = textFormatter;
            }
            else
            {
                this.amazonS3Options.Formatter = amazonS3Options.Formatter;
            }

            this.amazonS3Options.PathRoller = new PathRoller(amazonS3Options.Path, amazonS3Options.RollingInterval);
            this.amazonS3Options.Encoding = amazonS3Options.Encoding;
            this.amazonS3Options.FailureCallback = amazonS3Options.FailureCallback;
            this.amazonS3Options.BucketPath = amazonS3Options.BucketPath;
        }

        /// <summary>Emit a batch of log events, running asynchronously.</summary>
        /// <param name="batch">The batch of events to emit.</param>
        /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
        public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
        {
            if (batch is null)
            {
                throw new ArgumentNullException(nameof(batch));
            }

            var fileInformation = this.OpenFile();

            foreach (var logEvent in batch)
            {
                this.amazonS3Options.Formatter.Format(logEvent, fileInformation.OutputWriter);
            }

            await fileInformation.OutputWriter.FlushAsync();
            fileInformation.OutputWriter.Close();

            try
            {
                var result = await this.UploadFileToS3(fileInformation.FileName);
                SelfLog.WriteLine($"Uploaded data to Amazon S3 with result: {JsonConvert.SerializeObject(result)}.");
                File.Delete(fileInformation.FileName);
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine($"{ex.Message} {ex.StackTrace}");
                this.amazonS3Options.FailureCallback?.Invoke(ex);
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

            // ReSharper disable once InvertIf
            if (nextSequence || now >= this.amazonS3Options.NextCheckpoint.Value)
            {
                int? minSequence = null;

                // ReSharper disable once InvertIf
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
            var currentCheckpoint = this.amazonS3Options.PathRoller.GetCurrentCheckpoint(now);
            this.amazonS3Options.NextCheckpoint = this.amazonS3Options.PathRoller.GetNextCheckpoint(now);

            var existingFiles = Enumerable.Empty<string>();

            try
            {
                if (Directory.Exists(this.amazonS3Options.PathRoller.LogFileDirectory))
                {
                    existingFiles = Directory.GetFiles(this.amazonS3Options.PathRoller.LogFileDirectory, this.amazonS3Options.PathRoller.DirectorySearchPattern).Select(Path.GetFileName);
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

            if (minSequence != null)
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
            var client = this.amazonS3Options.AmazonS3Client;

            if (client is null)
            {
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (this.amazonS3Options.Endpoint != null)
                {
                    client = new AmazonS3Client(this.amazonS3Options.Endpoint);
                }
                else
                {
                    client = new AmazonS3Client(
                        new AmazonS3Config
                        {
                            ServiceURL = this.amazonS3Options.ServiceUrl
                        });
                }

                // In the case that awsAccessKeyId and awsSecretAccessKey is passed, we use it. Otherwise authorization is given by roles in AWS directly.
                if (!string.IsNullOrEmpty(this.amazonS3Options.AwsAccessKeyId) && !string.IsNullOrEmpty(this.amazonS3Options.AwsSecretAccessKey))
                {
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (this.amazonS3Options.Endpoint != null)
                    {
                        client = new AmazonS3Client(this.amazonS3Options.AwsAccessKeyId, this.amazonS3Options.AwsSecretAccessKey, this.amazonS3Options.Endpoint);
                    }
                    else
                    {
                        client = new AmazonS3Client(
                            this.amazonS3Options.AwsAccessKeyId,
                            this.amazonS3Options.AwsSecretAccessKey,
                            new AmazonS3Config
                            {
                                ServiceURL = this.amazonS3Options.ServiceUrl
                            });
                    }
                }
            }

            try
            {
                // S3 does not support updates, files are automatically rewritten so we will have to upload the entire file
                // Open the file for shared reading and writing
                using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                var key = string.IsNullOrWhiteSpace(this.amazonS3Options.BucketPath)
                              ? Path.GetFileName(fileName).Replace("\\", "/")
                              : Path.Combine(this.amazonS3Options.BucketPath, Path.GetFileName(fileName))
                                  .Replace("\\", "/");

                if (fs.Length is 0)
                {
                    return null;
                }

                var putRequest = new PutObjectRequest
                {
                    BucketName = this.amazonS3Options.BucketName,
                    Key = key,
                    InputStream = fs
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
}
