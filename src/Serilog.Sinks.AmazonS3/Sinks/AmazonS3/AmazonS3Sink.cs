// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AmazonS3Sink.cs" company="Haemmer Electronics">
// The project is licensed under the MIT license.
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

    using Core;
    using Events;
    using Formatting;

    /// <summary>
    ///     This class is the main class and contains all options for the AmazonS3 sink.
    /// </summary>
    public class AmazonS3Sink : ILogEventSink
    {
        /// <summary>   The sink. </summary>
        private readonly ILogEventSink sink;

        /// <summary>   Initializes a new instance of the <see cref="AmazonS3Sink" /> class. </summary>
        /// <exception cref="ArgumentNullException">    addSink or formatter or path. </exception>
        /// <exception cref="ArgumentException">
        ///     Negative value provided; file size limit must be
        ///     non-negative. - fileSizeLimitBytes or At least one
        ///     file must be retained. - retainedFileCountLimit or
        ///     Buffered writes are not available when file sharing
        ///     is enabled. - buffered or File lifecycle hooks are
        ///     not currently supported for shared log files. -
        ///     hooks.
        /// </exception>
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
        /// <param name="autoUploadEvents">         Automatically upload all events immediately. </param>
        /// <param name="failureCallback">          (Optional) The failure callback. </param>
        /// <param name="bucketPath">               (Optional) The Amazon S3 bucket path. </param>
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
            bool autoUploadEvents,
            Action<Exception> failureCallback = null,
            string bucketPath = null)
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
                autoUploadEvents,
                failureCallback,
                bucketPath);
        }

        /// <summary>   Initializes a new instance of the <see cref="AmazonS3Sink" /> class. </summary>
        /// <exception cref="ArgumentNullException">    addSink or formatter or path. </exception>
        /// <exception cref="ArgumentException">
        ///     Negative value provided; file size limit must be
        ///     non-negative. - fileSizeLimitBytes or At least one
        ///     file must be retained. - retainedFileCountLimit or
        ///     Buffered writes are not available when file sharing
        ///     is enabled. - buffered or File lifecycle hooks are
        ///     not currently supported for shared log files. -
        ///     hooks.
        /// </exception>
        /// <param name="formatter">                The formatter. </param>
        /// <param name="path">                     The path. </param>
        /// <param name="fileSizeLimitBytes">       The file size limit bytes. </param>
        /// <param name="buffered">                 if set to <c>true</c> [buffered]. </param>
        /// <param name="encoding">                 The encoding. </param>
        /// <param name="rollingInterval">          The rolling interval. </param>
        /// <param name="retainedFileCountLimit">   The retained file count limit. </param>
        /// <param name="hooks">                    The hooks. </param>
        /// <param name="bucketName">               The Amazon S3 bucket name. </param>
        /// <param name="serviceUrl">               The Amazon S3 service url. </param>
        /// <param name="awsAccessKeyId">           The Amazon S3 access key id. </param>
        /// <param name="awsSecretAccessKey">       The Amazon S3 secret access key. </param>
        /// <param name="autoUploadEvents">         Automatically upload all events immediately. </param>
        /// <param name="failureCallback">          (Optional) The failure callback. </param>
        /// <param name="bucketPath">               (Optional) The Amazon S3 bucket path. </param>
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
            string serviceUrl,
            string awsAccessKeyId,
            string awsSecretAccessKey,
            bool autoUploadEvents,
            Action<Exception> failureCallback = null,
            string bucketPath = null)
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
                serviceUrl,
                awsAccessKeyId,
                awsSecretAccessKey,
                autoUploadEvents,
                failureCallback,
                bucketPath);
        }

        /// <summary>   Initializes a new instance of the <see cref="AmazonS3Sink" /> class. </summary>
        /// <exception cref="ArgumentNullException">    addSink or formatter or path. </exception>
        /// <exception cref="ArgumentException">
        ///     Negative value provided; file size limit must be
        ///     non-negative. - fileSizeLimitBytes or At least one
        ///     file must be retained. - retainedFileCountLimit or
        ///     Buffered writes are not available when file sharing
        ///     is enabled. - buffered or File lifecycle hooks are
        ///     not currently supported for shared log files. -
        ///     hooks.
        /// </exception>
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
        /// <param name="autoUploadEvents">         Automatically upload all events immediately. </param>
        /// <param name="failureCallback">          (Optional) The failure callback. </param>
        /// <param name="bucketPath">               (Optional) The Amazon S3 bucket path. </param>
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
            bool autoUploadEvents,
            Action<Exception> failureCallback = null,
            string bucketPath = null)
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
                autoUploadEvents,
                failureCallback,
                bucketPath);
        }

        /// <summary>   Emit the provided log event to the sink. </summary>
        /// <param name="logEvent"> The log event to write. </param>
        /// <inheritdoc cref="ILogEventSink" />
        public void Emit(LogEvent logEvent)
        {
            this.sink.Emit(logEvent);
        }
    }
}