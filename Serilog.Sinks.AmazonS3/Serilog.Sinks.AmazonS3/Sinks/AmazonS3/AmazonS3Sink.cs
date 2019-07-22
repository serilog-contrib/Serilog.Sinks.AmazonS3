// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AmazonS3Sink.cs" company="Hämmer Electronics">
// The project is licensed under the GNU GENERAL PUBLIC LICENSE, Version 3, 29 June 2007
// </copyright>
// <summary>
//   Defines the AmazonS3SqlSink type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3
{
    using System;
    using System.Text;

    using Amazon;

    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Formatting;

    /// <summary>
    ///     This class is the main class and contains all options for the AmazonS3 sink.
    /// </summary>
    ///
    /// ### <inheritdoc cref="ILogEventSink"/>

    public class AmazonS3Sink : ILogEventSink
    {
        /// <summary>   The sink. </summary>
        private readonly ILogEventSink sink;

        /// <summary>   Initializes a new instance of the <see cref="AmazonS3Sink" /> class. </summary>
        ///
        /// <exception cref="ArgumentNullException">    addSink or formatter or path. </exception>
        /// <exception cref="ArgumentException">        Negative value provided; file size limit must be
        ///                                             non-negative. - fileSizeLimitBytes or At least one
        ///                                             file must be retained. - retainedFileCountLimit or
        ///                                             Buffered writes are not available when file sharing
        ///                                             is enabled. - buffered or File lifecycle hooks are
        ///                                             not currently supported for shared log files. -
        ///                                             hooks. </exception>
        ///
        /// <param name="formatter">                The formatter. </param>
        /// <param name="path">                     The path. </param>
        /// <param name="fileSizeLimitBytes">       The file size limit bytes. </param>
        /// <param name="buffered">                 if set to <c>true</c> [buffered]. </param>
        /// <param name="encoding">                 The encoding. </param>
        /// <param name="rollingInterval">          The rolling interval. </param>
        /// <param name="retainedFileCountLimit">   The retained file count limit. </param>
        /// <param name="hooks">                    The hooks. </param>
        /// <param name="bucketName">               The Amazon S3 bucket name. </param>
        /// <param name="endpoint">                 The Amazon S3 endpoint. </param>
        /// <param name="awsAccessKeyId">           The Amazon S3 access key id. </param>
        /// <param name="awsSecretAccessKey">       The Amazon S3 secret access key. </param>
        ///
        /// ### <returns>   A <see cref="LoggerConfiguration" /> to use with Serilog. </returns>

        public AmazonS3Sink(
            ITextFormatter formatter,
            string path,
            long? fileSizeLimitBytes,
            bool buffered,
            Encoding encoding,
            RollingInterval rollingInterval,
            int? retainedFileCountLimit,
            FileLifecycleHooks hooks,
            string bucketName,
            RegionEndpoint endpoint,
            string awsAccessKeyId,
            string awsSecretAccessKey,
            Action<Exception> failureCallback = null)
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

            this.sink = new RollingFileSink(
                    path,
                    formatter,
                    fileSizeLimitBytes,
                    retainedFileCountLimit,
                    encoding,
                    buffered,
                    rollingInterval,
                    true,
                    hooks,
                    bucketName,
                    endpoint,
                    awsAccessKeyId,
                    awsSecretAccessKey,
                    failureCallback);
        }

        /// <summary>   Initializes a new instance of the <see cref="AmazonS3Sink" /> class. </summary>
        ///
        /// <exception cref="ArgumentNullException">    addSink or formatter or path. </exception>
        /// <exception cref="ArgumentException">        Negative value provided; file size limit must be
        ///                                             non-negative. - fileSizeLimitBytes or At least one
        ///                                             file must be retained. - retainedFileCountLimit or
        ///                                             Buffered writes are not available when file sharing
        ///                                             is enabled. - buffered or File lifecycle hooks are
        ///                                             not currently supported for shared log files. -
        ///                                             hooks. </exception>
        ///
        /// <param name="formatter">                The formatter. </param>
        /// <param name="path">                     The path. </param>
        /// <param name="fileSizeLimitBytes">       The file size limit bytes. </param>
        /// <param name="buffered">                 if set to <c>true</c> [buffered]. </param>
        /// <param name="encoding">                 The encoding. </param>
        /// <param name="rollingInterval">          The rolling interval. </param>
        /// <param name="retainedFileCountLimit">   The retained file count limit. </param>
        /// <param name="hooks">                    The hooks. </param>
        /// <param name="bucketName">               The Amazon S3 bucket name. </param>
        /// <param name="endpoint">                 The Amazon S3 endpoint. </param>
        ///
        /// ### <returns>   A <see cref="LoggerConfiguration" /> to use with Serilog. </returns>

        public AmazonS3Sink(
            ITextFormatter formatter,
            string path,
            long? fileSizeLimitBytes,
            bool buffered,
            Encoding encoding,
            RollingInterval rollingInterval,
            int? retainedFileCountLimit,
            FileLifecycleHooks hooks,
            string bucketName,
            RegionEndpoint endpoint,
            Action<Exception> failureCallback = null)
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

            this.sink = new RollingFileSink(
                    path,
                    formatter,
                    fileSizeLimitBytes,
                    retainedFileCountLimit,
                    encoding,
                    buffered,
                    rollingInterval,
                    true,
                    hooks,
                    bucketName,
                    endpoint,
                    failureCallback);
        }

        /// <summary>   Emit the provided log event to the sink. </summary>
        ///
        /// <param name="logEvent"> The log event to write. </param>
        ///
        /// <inheritdoc cref="ILogEventSink"/>

        public void Emit(LogEvent logEvent)
        {
            this.sink.Emit(logEvent);
        }
    }
}