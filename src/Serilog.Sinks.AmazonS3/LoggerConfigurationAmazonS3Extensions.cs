// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoggerConfigurationAmazonS3Extensions.cs" company="Haemmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class contains the Amazon S3 logger configuration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    using Amazon;

    using Serilog.Configuration;
    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Formatting;
    using Serilog.Formatting.Display;
    using Serilog.Sinks.AmazonS3;

    /// <summary>   This class contains the Amazon S3 logger configuration. </summary>
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "Reviewed. Suppression is OK here.")]

    // ReSharper disable once UnusedMember.Global
    public static class LoggerConfigurationAmazonS3Extensions
    {
        /// <summary>   The default file size limit bytes. </summary>
        private const long DefaultFileSizeLimitBytes = 1L * 1024 * 1024 * 1024;

        /// <summary>   The default output template. </summary>
        private const string DefaultOutputTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        /// <summary>   The default retained file count limit. </summary>
        private const int DefaultRetainedFileCountLimit = 31;

        /// <summary>   Write log events to the specified file. </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are
        ///     null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when one or more arguments have
        ///     unsupported or illegal values.
        /// </exception>
        /// <param name="sinkConfiguration">        Logger sink configuration. </param>
        /// <param name="path">                     Path to the file. </param>
        /// <param name="bucketName">               The Amazon S3 bucket name. </param>
        /// <param name="endpoint">                 The Amazon S3 endpoint. </param>
        /// <param name="awsAccessKeyId">           The Amazon S3 access key id. </param>
        /// <param name="awsSecretAccessKey">       The Amazon S3 access key. </param>
        /// <param name="autoUploadEvents">         Automatically upload all events immediately. </param>
        /// <param name="restrictedToMinimumLevel">
        ///     (Optional)
        ///     The minimum level for
        ///     events passed through the sink. Ignored when
        ///     <paramref name="levelSwitch" /> is specified.
        /// </param>
        /// <param name="outputTemplate">
        ///     (Optional)
        ///     A message template describing the format used to
        ///     write to the sink.
        ///     the default is "{Timestamp:yyyy-MM-dd
        ///     HH:mm:ss.fff zzz} [{Level:u3}]
        ///     {Message:lj}{NewLine}{Exception}".
        /// </param>
        /// <param name="formatProvider">
        ///     (Optional)
        ///     Supplies culture-specific formatting information, or
        ///     null.
        /// </param>
        /// <param name="fileSizeLimitBytes">
        ///     (Optional)
        ///     The approximate maximum size, in bytes, to which a
        ///     log file will be allowed to grow.
        ///     For unrestricted growth, pass null. The default
        ///     is 1 GB. To avoid writing partial events, the
        ///     last event within the limit will be written in
        ///     full even if it exceeds the limit.
        /// </param>
        /// <param name="levelSwitch">
        ///     (Optional)
        ///     A switch allowing the pass-through minimum level
        ///     to be changed at runtime.
        /// </param>
        /// <param name="buffered">
        ///     (Optional)
        ///     Indicates if flushing to the output file can be
        ///     buffered or not. The default
        ///     is false.
        /// </param>
        /// <param name="rollingInterval">
        ///     (Optional)
        ///     The interval at which logging will roll over to a new
        ///     file.
        /// </param>
        /// <param name="retainedFileCountLimit">
        ///     (Optional)
        ///     The maximum number of log files that will be retained,
        ///     including the current log file. For unlimited
        ///     retention, pass null. The default is 31.
        /// </param>
        /// <param name="encoding">
        ///     (Optional)
        ///     Character encoding used to write the text file. The
        ///     default is UTF-8 without BOM.
        /// </param>
        /// <param name="hooks">
        ///     (Optional)
        ///     Optionally enables hooking into log file lifecycle
        ///     events.
        /// </param>
        /// <param name="failureCallback">          (Optional) The failure callback. </param>
        /// <param name="bucketPath">               (Optional) The Amazon S3 bucket path. </param>
        /// <returns>   Configuration object allowing method chaining. </returns>
        public static LoggerConfiguration AmazonS3(
            this LoggerSinkConfiguration sinkConfiguration,
            string path,
            string bucketName,
            RegionEndpoint endpoint,
            string awsAccessKeyId,
            string awsSecretAccessKey,
            bool autoUploadEvents = false,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string outputTemplate = DefaultOutputTemplate,
            IFormatProvider formatProvider = null,
            long? fileSizeLimitBytes = DefaultFileSizeLimitBytes,
            LoggingLevelSwitch levelSwitch = null,
            bool buffered = false,
            RollingInterval rollingInterval = RollingInterval.Day,
            int? retainedFileCountLimit = DefaultRetainedFileCountLimit,
            Encoding encoding = null,
            FileLifecycleHooks hooks = null,
            Action<Exception> failureCallback = null,
            string bucketPath = null)
        {
            if (sinkConfiguration == null)
            {
                throw new ArgumentNullException(nameof(sinkConfiguration));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            if (string.IsNullOrWhiteSpace(awsAccessKeyId))
            {
                throw new ArgumentNullException(nameof(awsAccessKeyId));
            }

            if (string.IsNullOrWhiteSpace(awsSecretAccessKey))
            {
                throw new ArgumentNullException(nameof(awsSecretAccessKey));
            }

            if (retainedFileCountLimit.HasValue && retainedFileCountLimit < 1)
            {
                throw new ArgumentException(
                    "Zero or negative value provided; retained file count limit must be at least 1.");
            }

            if (outputTemplate == null)
            {
                throw new ArgumentNullException(nameof(outputTemplate));
            }

            var formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            return sinkConfiguration.Sink(
                new AmazonS3Sink(
                    formatter,
                    path,
                    fileSizeLimitBytes,
                    buffered,
                    encoding,
                    rollingInterval,
                    retainedFileCountLimit,
                    hooks,
                    bucketName,
                    endpoint,
                    awsAccessKeyId,
                    awsSecretAccessKey,
                    autoUploadEvents,
                    failureCallback,
                    bucketPath),
                restrictedToMinimumLevel,
                levelSwitch);
        }

        /// <summary>   Write log events to the specified file. </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are
        ///     null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when one or more arguments have
        ///     unsupported or illegal values.
        /// </exception>
        /// <param name="sinkConfiguration">        Logger sink configuration. </param>
        /// <param name="path">                     Path to the file. </param>
        /// <param name="bucketName">               The Amazon S3 bucket name. </param>
        /// <param name="serviceUrl">               The Amazon S3 service url. </param>
        /// <param name="awsAccessKeyId">           The Amazon S3 access key id. </param>
        /// <param name="awsSecretAccessKey">       The Amazon S3 access key. </param>
        /// <param name="autoUploadEvents">         Automatically upload all events immediately. </param>
        /// <param name="restrictedToMinimumLevel">
        ///     (Optional)
        ///     The minimum level for
        ///     events passed through the sink. Ignored when
        ///     <paramref name="levelSwitch" /> is specified.
        /// </param>
        /// <param name="outputTemplate">
        ///     (Optional)
        ///     A message template describing the format used to
        ///     write to the sink.
        ///     the default is "{Timestamp:yyyy-MM-dd
        ///     HH:mm:ss.fff zzz} [{Level:u3}]
        ///     {Message:lj}{NewLine}{Exception}".
        /// </param>
        /// <param name="formatProvider">
        ///     (Optional)
        ///     Supplies culture-specific formatting information, or
        ///     null.
        /// </param>
        /// <param name="fileSizeLimitBytes">
        ///     (Optional)
        ///     The approximate maximum size, in bytes, to which a
        ///     log file will be allowed to grow.
        ///     For unrestricted growth, pass null. The default
        ///     is 1 GB. To avoid writing partial events, the
        ///     last event within the limit will be written in
        ///     full even if it exceeds the limit.
        /// </param>
        /// <param name="levelSwitch">
        ///     (Optional)
        ///     A switch allowing the pass-through minimum level
        ///     to be changed at runtime.
        /// </param>
        /// <param name="buffered">
        ///     (Optional)
        ///     Indicates if flushing to the output file can be
        ///     buffered or not. The default
        ///     is false.
        /// </param>
        /// <param name="rollingInterval">
        ///     (Optional)
        ///     The interval at which logging will roll over to a new
        ///     file.
        /// </param>
        /// <param name="retainedFileCountLimit">
        ///     (Optional)
        ///     The maximum number of log files that will be retained,
        ///     including the current log file. For unlimited
        ///     retention, pass null. The default is 31.
        /// </param>
        /// <param name="encoding">
        ///     (Optional)
        ///     Character encoding used to write the text file. The
        ///     default is UTF-8 without BOM.
        /// </param>
        /// <param name="hooks">
        ///     (Optional)
        ///     Optionally enables hooking into log file lifecycle
        ///     events.
        /// </param>
        /// <param name="failureCallback">          (Optional) The failure callback. </param>
        /// <param name="bucketPath">               (Optional) The Amazon S3 bucket path. </param>
        /// <returns>   Configuration object allowing method chaining. </returns>
        public static LoggerConfiguration AmazonS3(
            this LoggerSinkConfiguration sinkConfiguration,
            string path,
            string bucketName,
            string serviceUrl,
            string awsAccessKeyId,
            string awsSecretAccessKey,
            bool autoUploadEvents = false,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string outputTemplate = DefaultOutputTemplate,
            IFormatProvider formatProvider = null,
            long? fileSizeLimitBytes = DefaultFileSizeLimitBytes,
            LoggingLevelSwitch levelSwitch = null,
            bool buffered = false,
            RollingInterval rollingInterval = RollingInterval.Day,
            int? retainedFileCountLimit = DefaultRetainedFileCountLimit,
            Encoding encoding = null,
            FileLifecycleHooks hooks = null,
            Action<Exception> failureCallback = null,
            string bucketPath = null)
        {
            if (sinkConfiguration == null)
            {
                throw new ArgumentNullException(nameof(sinkConfiguration));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            if (serviceUrl == null)
            {
                throw new ArgumentNullException(nameof(serviceUrl));
            }

            if (string.IsNullOrWhiteSpace(awsAccessKeyId))
            {
                throw new ArgumentNullException(nameof(awsAccessKeyId));
            }

            if (string.IsNullOrWhiteSpace(awsSecretAccessKey))
            {
                throw new ArgumentNullException(nameof(awsSecretAccessKey));
            }

            if (retainedFileCountLimit.HasValue && retainedFileCountLimit < 1)
            {
                throw new ArgumentException(
                    "Zero or negative value provided; retained file count limit must be at least 1.");
            }

            if (outputTemplate == null)
            {
                throw new ArgumentNullException(nameof(outputTemplate));
            }

            var formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            return sinkConfiguration.Sink(
                new AmazonS3Sink(
                    formatter,
                    path,
                    fileSizeLimitBytes,
                    buffered,
                    encoding,
                    rollingInterval,
                    retainedFileCountLimit,
                    hooks,
                    bucketName,
                    serviceUrl,
                    awsAccessKeyId,
                    awsSecretAccessKey,
                    autoUploadEvents,
                    failureCallback,
                    bucketPath),
                restrictedToMinimumLevel,
                levelSwitch);
        }

        /// <summary>   Write log events to the specified file. </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are
        ///     null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when one or more arguments have
        ///     unsupported or illegal values.
        /// </exception>
        /// <param name="sinkConfiguration">        Logger sink configuration. </param>
        /// <param name="formatter">                The formatter.</param>
        /// <param name="path">                     Path to the file. </param>
        /// <param name="bucketName">               The Amazon S3 bucket name. </param>
        /// <param name="endpoint">                 The Amazon S3 endpoint. </param>
        /// <param name="awsAccessKeyId">           The Amazon S3 access key id. </param>
        /// <param name="awsSecretAccessKey">       The Amazon S3 access key. </param>
        /// <param name="autoUploadEvents">         Automatically upload all events immediately. </param>
        /// <param name="restrictedToMinimumLevel">
        ///     (Optional)
        ///     The minimum level for
        ///     events passed through the sink. Ignored when
        ///     <paramref name="levelSwitch" /> is specified.
        /// </param>
        /// <param name="fileSizeLimitBytes">
        ///     (Optional)
        ///     The approximate maximum size, in bytes, to which a
        ///     log file will be allowed to grow.
        ///     For unrestricted growth, pass null. The default
        ///     is 1 GB. To avoid writing partial events, the
        ///     last event within the limit will be written in
        ///     full even if it exceeds the limit.
        /// </param>
        /// <param name="levelSwitch">
        ///     (Optional)
        ///     A switch allowing the pass-through minimum level
        ///     to be changed at runtime.
        /// </param>
        /// <param name="buffered">
        ///     (Optional)
        ///     Indicates if flushing to the output file can be
        ///     buffered or not. The default
        ///     is false.
        /// </param>
        /// <param name="rollingInterval">
        ///     (Optional)
        ///     The interval at which logging will roll over to a new
        ///     file.
        /// </param>
        /// <param name="retainedFileCountLimit">
        ///     (Optional)
        ///     The maximum number of log files that will be retained,
        ///     including the current log file. For unlimited
        ///     retention, pass null. The default is 31.
        /// </param>
        /// <param name="encoding">
        ///     (Optional)
        ///     Character encoding used to write the text file. The
        ///     default is UTF-8 without BOM.
        /// </param>
        /// <param name="hooks">
        ///     (Optional)
        ///     Optionally enables hooking into log file lifecycle
        ///     events.
        /// </param>
        /// <param name="failureCallback">          (Optional) The failure callback. </param>
        /// <param name="bucketPath">               (Optional) The Amazon S3 bucket path. </param>
        /// <returns>   Configuration object allowing method chaining. </returns>
        public static LoggerConfiguration AmazonS3(
            this LoggerSinkConfiguration sinkConfiguration,
            ITextFormatter formatter,
            string path,
            string bucketName,
            RegionEndpoint endpoint,
            string awsAccessKeyId,
            string awsSecretAccessKey,
            bool autoUploadEvents = false,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            long? fileSizeLimitBytes = DefaultFileSizeLimitBytes,
            LoggingLevelSwitch levelSwitch = null,
            bool buffered = false,
            RollingInterval rollingInterval = RollingInterval.Day,
            int? retainedFileCountLimit = DefaultRetainedFileCountLimit,
            Encoding encoding = null,
            FileLifecycleHooks hooks = null,
            Action<Exception> failureCallback = null,
            string bucketPath = null)
        {
            if (sinkConfiguration == null)
            {
                throw new ArgumentNullException(nameof(sinkConfiguration));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            if (string.IsNullOrWhiteSpace(awsAccessKeyId))
            {
                throw new ArgumentNullException(nameof(awsAccessKeyId));
            }

            if (string.IsNullOrWhiteSpace(awsSecretAccessKey))
            {
                throw new ArgumentNullException(nameof(awsSecretAccessKey));
            }

            if (retainedFileCountLimit.HasValue && retainedFileCountLimit < 1)
            {
                throw new ArgumentException(
                    "Zero or negative value provided; retained file count limit must be at least 1.");
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            return sinkConfiguration.Sink(
                new AmazonS3Sink(
                    formatter,
                    path,
                    fileSizeLimitBytes,
                    buffered,
                    encoding,
                    rollingInterval,
                    retainedFileCountLimit,
                    hooks,
                    bucketName,
                    endpoint,
                    awsAccessKeyId,
                    awsSecretAccessKey,
                    autoUploadEvents,
                    failureCallback,
                    bucketPath),
                restrictedToMinimumLevel,
                levelSwitch);
        }

        /// <summary>   Write log events to the specified file. </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are
        ///     null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when one or more arguments have
        ///     unsupported or illegal values.
        /// </exception>
        /// <param name="sinkConfiguration">        Logger sink configuration. </param>
        /// <param name="path">                     Path to the file. </param>
        /// <param name="bucketName">               The Amazon S3 bucket name. </param>
        /// <param name="endpoint">                 The Amazon S3 endpoint. </param>
        /// <param name="autoUploadEvents">         Automatically upload all events immediately. </param>
        /// <param name="restrictedToMinimumLevel">
        ///     (Optional)
        ///     The minimum level for
        ///     events passed through the sink. Ignored when
        ///     <paramref name="levelSwitch" /> is specified.
        /// </param>
        /// <param name="outputTemplate">
        ///     (Optional)
        ///     A message template describing the format used to
        ///     write to the sink.
        ///     the default is "{Timestamp:yyyy-MM-dd
        ///     HH:mm:ss.fff zzz} [{Level:u3}]
        ///     {Message:lj}{NewLine}{Exception}".
        /// </param>
        /// <param name="formatProvider">
        ///     (Optional)
        ///     Supplies culture-specific formatting information, or
        ///     null.
        /// </param>
        /// <param name="fileSizeLimitBytes">
        ///     (Optional)
        ///     The approximate maximum size, in bytes, to which a
        ///     log file will be allowed to grow.
        ///     For unrestricted growth, pass null. The default
        ///     is 1 GB. To avoid writing partial events, the
        ///     last event within the limit will be written in
        ///     full even if it exceeds the limit.
        /// </param>
        /// <param name="levelSwitch">
        ///     (Optional)
        ///     A switch allowing the pass-through minimum level
        ///     to be changed at runtime.
        /// </param>
        /// <param name="buffered">
        ///     (Optional)
        ///     Indicates if flushing to the output file can be
        ///     buffered or not. The default
        ///     is false.
        /// </param>
        /// <param name="rollingInterval">
        ///     (Optional)
        ///     The interval at which logging will roll over to a new
        ///     file.
        /// </param>
        /// <param name="retainedFileCountLimit">
        ///     (Optional)
        ///     The maximum number of log files that will be retained,
        ///     including the current log file. For unlimited
        ///     retention, pass null. The default is 31.
        /// </param>
        /// <param name="encoding">
        ///     (Optional)
        ///     Character encoding used to write the text file. The
        ///     default is UTF-8 without BOM.
        /// </param>
        /// <param name="hooks">
        ///     (Optional)
        ///     Optionally enables hooking into log file lifecycle
        ///     events.
        /// </param>
        /// <param name="failureCallback">          (Optional) The failure callback. </param>
        /// <param name="bucketPath">               (Optional) The Amazon S3 bucket path. </param>
        /// <returns>   Configuration object allowing method chaining. </returns>
        public static LoggerConfiguration AmazonS3(
            this LoggerSinkConfiguration sinkConfiguration,
            string path,
            string bucketName,
            RegionEndpoint endpoint,
            bool autoUploadEvents = false,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string outputTemplate = DefaultOutputTemplate,
            IFormatProvider formatProvider = null,
            long? fileSizeLimitBytes = DefaultFileSizeLimitBytes,
            LoggingLevelSwitch levelSwitch = null,
            bool buffered = false,
            RollingInterval rollingInterval = RollingInterval.Day,
            int? retainedFileCountLimit = DefaultRetainedFileCountLimit,
            Encoding encoding = null,
            FileLifecycleHooks hooks = null,
            Action<Exception> failureCallback = null,
            string bucketPath = null)
        {
            if (sinkConfiguration == null)
            {
                throw new ArgumentNullException(nameof(sinkConfiguration));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            if (retainedFileCountLimit.HasValue && retainedFileCountLimit < 1)
            {
                throw new ArgumentException(
                    "Zero or negative value provided; retained file count limit must be at least 1.");
            }

            if (outputTemplate == null)
            {
                throw new ArgumentNullException(nameof(outputTemplate));
            }

            var formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            return sinkConfiguration.Sink(
                new AmazonS3Sink(
                    formatter,
                    path,
                    fileSizeLimitBytes,
                    buffered,
                    encoding,
                    rollingInterval,
                    retainedFileCountLimit,
                    hooks,
                    bucketName,
                    endpoint,
                    autoUploadEvents,
                    failureCallback,
                    bucketPath),
                restrictedToMinimumLevel,
                levelSwitch);
        }

        /// <summary>   Write log events to the specified file. </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are
        ///     null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when one or more arguments have
        ///     unsupported or illegal values.
        /// </exception>
        /// <param name="sinkConfiguration">        Logger sink configuration. </param>
        /// <param name="formatter">                The formatter.</param>
        /// <param name="path">                     Path to the file. </param>
        /// <param name="bucketName">               The Amazon S3 bucket name. </param>
        /// <param name="endpoint">                 The Amazon S3 endpoint. </param>
        /// <param name="autoUploadEvents">         Automatically upload all events immediately. </param>
        /// <param name="restrictedToMinimumLevel">
        ///     (Optional)
        ///     The minimum level for
        ///     events passed through the sink. Ignored when
        ///     <paramref name="levelSwitch" /> is specified.
        /// </param>
        /// <param name="fileSizeLimitBytes">
        ///     (Optional)
        ///     The approximate maximum size, in bytes, to which a
        ///     log file will be allowed to grow.
        ///     For unrestricted growth, pass null. The default
        ///     is 1 GB. To avoid writing partial events, the
        ///     last event within the limit will be written in
        ///     full even if it exceeds the limit.
        /// </param>
        /// <param name="levelSwitch">
        ///     (Optional)
        ///     A switch allowing the pass-through minimum level
        ///     to be changed at runtime.
        /// </param>
        /// <param name="buffered">
        ///     (Optional)
        ///     Indicates if flushing to the output file can be
        ///     buffered or not. The default
        ///     is false.
        /// </param>
        /// <param name="rollingInterval">
        ///     (Optional)
        ///     The interval at which logging will roll over to a new
        ///     file.
        /// </param>
        /// <param name="retainedFileCountLimit">
        ///     (Optional)
        ///     The maximum number of log files that will be retained,
        ///     including the current log file. For unlimited
        ///     retention, pass null. The default is 31.
        /// </param>
        /// <param name="encoding">
        ///     (Optional)
        ///     Character encoding used to write the text file. The
        ///     default is UTF-8 without BOM.
        /// </param>
        /// <param name="hooks">
        ///     (Optional)
        ///     Optionally enables hooking into log file lifecycle
        ///     events.
        /// </param>
        /// <param name="failureCallback">          (Optional) The failure callback. </param>
        /// <param name="bucketPath">               (Optional) The Amazon S3 bucket path. </param>
        /// <returns>   Configuration object allowing method chaining. </returns>
        public static LoggerConfiguration AmazonS3(
            this LoggerSinkConfiguration sinkConfiguration,
            ITextFormatter formatter,
            string path,
            string bucketName,
            RegionEndpoint endpoint,
            bool autoUploadEvents = false,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            long? fileSizeLimitBytes = DefaultFileSizeLimitBytes,
            LoggingLevelSwitch levelSwitch = null,
            bool buffered = false,
            RollingInterval rollingInterval = RollingInterval.Day,
            int? retainedFileCountLimit = DefaultRetainedFileCountLimit,
            Encoding encoding = null,
            FileLifecycleHooks hooks = null,
            Action<Exception> failureCallback = null,
            string bucketPath = null)
        {
            if (sinkConfiguration == null)
            {
                throw new ArgumentNullException(nameof(sinkConfiguration));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            if (retainedFileCountLimit.HasValue && retainedFileCountLimit < 1)
            {
                throw new ArgumentException(
                    "Zero or negative value provided; retained file count limit must be at least 1.");
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            return sinkConfiguration.Sink(
                new AmazonS3Sink(
                    formatter,
                    path,
                    fileSizeLimitBytes,
                    buffered,
                    encoding,
                    rollingInterval,
                    retainedFileCountLimit,
                    hooks,
                    bucketName,
                    endpoint,
                    autoUploadEvents,
                    failureCallback,
                    bucketPath),
                restrictedToMinimumLevel,
                levelSwitch);
        }
    }
}