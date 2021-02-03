// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoggerConfigurationAmazonS3Extensions.cs" company="Hämmer Electronics">
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
    using Amazon.S3;

    using Serilog.Configuration;
    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Formatting;
    using Serilog.Sinks.AmazonS3;
    using Serilog.Sinks.PeriodicBatching;

    /// <summary>
    /// This class contains the Amazon S3 logger configuration.
    /// </summary>
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "Reviewed. Suppression is OK here.")]

    // ReSharper disable once UnusedMember.Global
    public static class LoggerConfigurationAmazonS3Extensions
    {
        /// <summary>
        /// The default output template.
        /// </summary>
        private const string DefaultOutputTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        /// <summary>
        /// The default batch size limit.
        /// </summary>
        private const int DefaultBatchSizeLimit = 100;

        /// <summary>
        /// The default value to eagerly emit the first event.
        /// </summary>
        private const bool DefaultEagerlyEmitFirstEvent = true;

        /// <summary>
        /// The default queue size limit.
        /// </summary>
        private const int DefaultQueueSizeLimit = 10000;

        /// <summary>
        /// The default encoding.
        /// </summary>
        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        /// <summary>
        /// The default batching period.
        /// </summary>
        private static readonly TimeSpan DefaultBatchingPeriod = TimeSpan.FromSeconds(2);

        /// <summary>Write log events to the specified file.</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one or more required arguments are
        /// null.
        /// </exception>
        /// <param name="sinkConfiguration">The logger sink configuration.</param>
        /// <param name="path">The path to the file</param>
        /// <param name="bucketName">The Amazon S3 bucket name.</param>
        /// <param name="endpoint">The Amazon S3 endpoint.</param>
        /// <param name="awsAccessKeyId">The Amazon S3 access key id.</param>
        /// <param name="awsSecretAccessKey">The Amazon S3 access key.</param>
        /// <param name="restrictedToMinimumLevel">
        /// (Optional)
        /// The minimum level for
        /// events passed through the sink. Ignored when
        /// <paramref name="levelSwitch" /> is specified.
        /// </param>
        /// <param name="outputTemplate">The output template.</param>
        /// <param name="formatProvider">
        /// (Optional)
        /// Supplies culture-specific formatting information, or
        /// null.
        /// </param>
        /// <param name="levelSwitch">
        /// (Optional)
        /// A switch allowing the pass-through minimum level
        /// to be changed at runtime.
        /// </param>
        /// <param name="rollingInterval">
        /// (Optional)
        /// The interval at which logging will roll over to a new
        /// file.
        /// </param>
        /// <param name="encoding">
        /// (Optional)
        /// Character encoding used to write the text file. The
        /// default is UTF-8 without BOM.
        /// </param>
        /// <param name="failureCallback">The failure callback.</param>
        /// <param name="bucketPath">The Amazon S3 bucket path.</param>
        /// <param name="batchSizeLimit">The batch size limit.</param>
        /// <param name="batchingPeriod">The batching period.</param>
        /// <param name="eagerlyEmitFirstEvent">A value indicating whether the first event should be emitted immediately or not.</param>
        /// <param name="queueSizeLimit">The queue size limit.</param>
        /// <returns>The configuration object allowing method chaining.</returns>
        public static LoggerConfiguration AmazonS3(
            this LoggerSinkConfiguration sinkConfiguration,
            string path,
            string bucketName,
            RegionEndpoint endpoint,
            string awsAccessKeyId,
            string awsSecretAccessKey,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string outputTemplate = DefaultOutputTemplate,
            IFormatProvider formatProvider = null,
            LoggingLevelSwitch levelSwitch = null,
            RollingInterval rollingInterval = RollingInterval.Day,
            Encoding encoding = null,
            Action<Exception> failureCallback = null,
            string bucketPath = null,
            int? batchSizeLimit = DefaultBatchSizeLimit,
            TimeSpan? batchingPeriod = null,
            bool? eagerlyEmitFirstEvent = DefaultEagerlyEmitFirstEvent,
            int? queueSizeLimit = DefaultQueueSizeLimit)
        {
            if (sinkConfiguration is null)
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

            if (endpoint is null)
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

            if (string.IsNullOrWhiteSpace(outputTemplate))
            {
                outputTemplate = DefaultOutputTemplate;
            }

            if (encoding is null)
            {
                encoding = DefaultEncoding;
            }

            if (batchingPeriod is null)
            {
                batchingPeriod = DefaultBatchingPeriod;
            }

            var options = new AmazonS3Options
            {
                Path = path,
                BucketName = bucketName,
                Endpoint = endpoint,
                AwsAccessKeyId = awsAccessKeyId,
                AwsSecretAccessKey = awsSecretAccessKey,
                OutputTemplate = outputTemplate,
                FormatProvider = formatProvider,
                RollingInterval = rollingInterval,
                Encoding = encoding,
                FailureCallback = failureCallback,
                BucketPath = bucketPath
            };

            var amazonS3Sink = new AmazonS3Sink(options);

            var batchingOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = batchSizeLimit ?? DefaultBatchSizeLimit,
                Period = (TimeSpan)batchingPeriod,
                EagerlyEmitFirstEvent = eagerlyEmitFirstEvent ?? DefaultEagerlyEmitFirstEvent,
                QueueLimit = queueSizeLimit ?? DefaultQueueSizeLimit
            };

            var batchingSink = new PeriodicBatchingSink(amazonS3Sink, batchingOptions);
            return sinkConfiguration.Sink(batchingSink, restrictedToMinimumLevel, levelSwitch);
        }

        /// <summary>Write log events to the specified file.</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one or more required arguments are
        /// null.
        /// </exception>
        /// <param name="sinkConfiguration">The logger sink configuration.</param>
        /// <param name="path">The path to the file.</param>
        /// <param name="bucketName">The Amazon S3 bucket name.</param>
        /// <param name="endpoint">The Amazon S3 endpoint.</param>
        /// <param name="awsAccessKeyId">The Amazon S3 access key id.</param>
        /// <param name="awsSecretAccessKey">The Amazon S3 access key.</param>
        /// <param name="restrictedToMinimumLevel">
        /// (Optional)
        /// The minimum level for
        /// events passed through the sink. Ignored when
        /// <paramref name="levelSwitch" /> is specified.
        /// </param>
        /// <param name="levelSwitch">
        /// (Optional)
        /// A switch allowing the pass-through minimum level
        /// to be changed at runtime.
        /// </param>
        /// <param name="formatter">The formatter.</param>
        /// <param name="rollingInterval">
        /// (Optional)
        /// The interval at which logging will roll over to a new
        /// file.
        /// </param>
        /// <param name="encoding">
        /// (Optional)
        /// Character encoding used to write the text file. The
        /// default is UTF-8 without BOM.
        /// </param>
        /// <param name="failureCallback">The failure callback.</param>
        /// <param name="bucketPath">The Amazon S3 bucket path.</param>
        /// <param name="batchSizeLimit">The batch size limit.</param>
        /// <param name="batchingPeriod">The batching period.</param>
        /// <param name="eagerlyEmitFirstEvent">A value indicating whether the first event should be emitted immediately or not.</param>
        /// <param name="queueSizeLimit">The queue size limit.</param>
        /// <returns>The configuration object allowing method chaining.</returns>
        public static LoggerConfiguration AmazonS3(
                this LoggerSinkConfiguration sinkConfiguration,
                string path,
                string bucketName,
                RegionEndpoint endpoint,
                string awsAccessKeyId,
                string awsSecretAccessKey,
                LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
                LoggingLevelSwitch levelSwitch = null,
                ITextFormatter formatter = null,
                RollingInterval rollingInterval = RollingInterval.Day,
                Encoding encoding = null,
                Action<Exception> failureCallback = null,
                string bucketPath = null,
                int? batchSizeLimit = DefaultBatchSizeLimit,
                TimeSpan? batchingPeriod = null,
                bool? eagerlyEmitFirstEvent = DefaultEagerlyEmitFirstEvent,
                int? queueSizeLimit = DefaultQueueSizeLimit)
        {
            if (sinkConfiguration is null)
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

            if (endpoint is null)
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

            if (formatter is null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (encoding is null)
            {
                encoding = DefaultEncoding;
            }

            if (batchingPeriod is null)
            {
                batchingPeriod = DefaultBatchingPeriod;
            }

            var options = new AmazonS3Options
            {
                Path = path,
                BucketName = bucketName,
                Endpoint = endpoint,
                AwsAccessKeyId = awsAccessKeyId,
                AwsSecretAccessKey = awsSecretAccessKey,
                Formatter = formatter,
                RollingInterval = rollingInterval,
                Encoding = encoding,
                FailureCallback = failureCallback,
                BucketPath = bucketPath
            };

            var amazonS3Sink = new AmazonS3Sink(options);

            var batchingOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = batchSizeLimit ?? DefaultBatchSizeLimit,
                Period = (TimeSpan)batchingPeriod,
                EagerlyEmitFirstEvent = eagerlyEmitFirstEvent ?? DefaultEagerlyEmitFirstEvent,
                QueueLimit = queueSizeLimit ?? DefaultQueueSizeLimit
            };

            var batchingSink = new PeriodicBatchingSink(amazonS3Sink, batchingOptions);
            return sinkConfiguration.Sink(batchingSink, restrictedToMinimumLevel, levelSwitch);
        }

        /// <summary>Write log events to the specified file.</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one or more required arguments are
        /// null.
        /// </exception>
        /// <param name="sinkConfiguration">The logger sink configuration.</param>
        /// <param name="path">The path to the file.</param>
        /// <param name="bucketName">The Amazon S3 bucket name.</param>
        /// <param name="endpoint">The Amazon S3 endpoint.</param>
        /// <param name="restrictedToMinimumLevel">
        /// (Optional)
        /// The minimum level for
        /// events passed through the sink. Ignored when
        /// <paramref name="levelSwitch" /> is specified.
        /// </param>
        /// <param name="levelSwitch">
        /// (Optional)
        /// A switch allowing the pass-through minimum level
        /// to be changed at runtime.
        /// </param>
        /// <param name="outputTemplate">
        /// (Optional)
        /// A message template describing the format used to
        /// write to the sink.
        /// The default is "{Timestamp:yyyy-MM-dd
        /// HH:mm:ss.fff zzz} [{Level:u3}]
        /// {Message:lj}{NewLine}{Exception}".
        /// </param>
        /// <param name="formatProvider">
        /// (Optional)
        /// Supplies culture-specific formatting information, or
        /// null.
        /// </param>
        /// <param name="rollingInterval">
        /// (Optional)
        /// The interval at which logging will roll over to a new
        /// file.
        /// </param>
        /// <param name="encoding">
        /// (Optional)
        /// Character encoding used to write the text file. The
        /// default is UTF-8 without BOM.
        /// </param>
        /// <param name="failureCallback">The failure callback.</param>
        /// <param name="bucketPath">The Amazon S3 bucket path.</param>
        /// <param name="batchSizeLimit">The batch size limit.</param>
        /// <param name="batchingPeriod">The batching period.</param>
        /// <param name="eagerlyEmitFirstEvent">A value indicating whether the first event should be emitted immediately or not.</param>
        /// <param name="queueSizeLimit">The queue size limit.</param>
        /// <returns>The configuration object allowing method chaining.</returns>
        public static LoggerConfiguration AmazonS3(
            this LoggerSinkConfiguration sinkConfiguration,
            string path,
            string bucketName,
            RegionEndpoint endpoint,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch levelSwitch = null,
            string outputTemplate = DefaultOutputTemplate,
            IFormatProvider formatProvider = null,
            RollingInterval rollingInterval = RollingInterval.Day,
            Encoding encoding = null,
            Action<Exception> failureCallback = null,
            string bucketPath = null,
            int? batchSizeLimit = DefaultBatchSizeLimit,
            TimeSpan? batchingPeriod = null,
            bool? eagerlyEmitFirstEvent = DefaultEagerlyEmitFirstEvent,
            int? queueSizeLimit = DefaultQueueSizeLimit)
        {
            if (sinkConfiguration is null)
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

            if (endpoint is null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            if (string.IsNullOrWhiteSpace(outputTemplate))
            {
                outputTemplate = DefaultOutputTemplate;
            }

            if (encoding is null)
            {
                encoding = DefaultEncoding;
            }

            if (batchingPeriod is null)
            {
                batchingPeriod = DefaultBatchingPeriod;
            }

            var options = new AmazonS3Options
            {
                Path = path,
                BucketName = bucketName,
                Endpoint = endpoint,
                OutputTemplate = outputTemplate,
                FormatProvider = formatProvider,
                RollingInterval = rollingInterval,
                Encoding = encoding,
                FailureCallback = failureCallback,
                BucketPath = bucketPath
            };

            var amazonS3Sink = new AmazonS3Sink(options);

            var batchingOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = batchSizeLimit ?? DefaultBatchSizeLimit,
                Period = (TimeSpan)batchingPeriod,
                EagerlyEmitFirstEvent = eagerlyEmitFirstEvent ?? DefaultEagerlyEmitFirstEvent,
                QueueLimit = queueSizeLimit ?? DefaultQueueSizeLimit
            };

            var batchingSink = new PeriodicBatchingSink(amazonS3Sink, batchingOptions);
            return sinkConfiguration.Sink(batchingSink, restrictedToMinimumLevel, levelSwitch);
        }

        /// <summary>Write log events to the specified file.</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one or more required arguments are
        /// null.
        /// </exception>
        /// <param name="sinkConfiguration">The logger sink configuration.</param>
        /// <param name="path">The path to the file.</param>
        /// <param name="bucketName">The Amazon S3 bucket name.</param>
        /// <param name="endpoint">The Amazon S3 endpoint.</param>
        /// <param name="restrictedToMinimumLevel">
        /// (Optional)
        /// The minimum level for
        /// events passed through the sink. Ignored when
        /// <paramref name="levelSwitch" /> is specified.
        /// </param>
        /// <param name="levelSwitch">
        /// (Optional)
        /// A switch allowing the pass-through minimum level
        /// to be changed at runtime.
        /// </param>
        /// <param name="formatter">The formatter.</param>
        /// <param name="rollingInterval">
        /// (Optional)
        /// The interval at which logging will roll over to a new
        /// file.
        /// </param>
        /// <param name="encoding">
        /// (Optional)
        /// Character encoding used to write the text file. The
        /// default is UTF-8 without BOM.
        /// </param>
        /// <param name="failureCallback">The failure callback.</param>
        /// <param name="bucketPath">The Amazon S3 bucket path.</param>
        /// <param name="batchSizeLimit">The batch size limit.</param>
        /// <param name="batchingPeriod">The batching period.</param>
        /// <param name="eagerlyEmitFirstEvent">A value indicating whether the first event should be emitted immediately or not.</param>
        /// <param name="queueSizeLimit">The queue size limit.</param>
        /// <returns>The configuration object allowing method chaining.</returns>
        public static LoggerConfiguration AmazonS3(
            this LoggerSinkConfiguration sinkConfiguration,
            string path,
            string bucketName,
            RegionEndpoint endpoint,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch levelSwitch = null,
            ITextFormatter formatter = null,
            RollingInterval rollingInterval = RollingInterval.Day,
            Encoding encoding = null,
            Action<Exception> failureCallback = null,
            string bucketPath = null,
            int? batchSizeLimit = DefaultBatchSizeLimit,
            TimeSpan? batchingPeriod = null,
            bool? eagerlyEmitFirstEvent = DefaultEagerlyEmitFirstEvent,
            int? queueSizeLimit = DefaultQueueSizeLimit)
        {
            if (sinkConfiguration is null)
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

            if (endpoint is null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            if (formatter is null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (encoding is null)
            {
                encoding = DefaultEncoding;
            }

            if (batchingPeriod is null)
            {
                batchingPeriod = DefaultBatchingPeriod;
            }

            var options = new AmazonS3Options
            {
                Path = path,
                BucketName = bucketName,
                Endpoint = endpoint,
                Formatter = formatter,
                RollingInterval = rollingInterval,
                Encoding = encoding,
                FailureCallback = failureCallback,
                BucketPath = bucketPath
            };

            var amazonS3Sink = new AmazonS3Sink(options);

            var batchingOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = batchSizeLimit ?? DefaultBatchSizeLimit,
                Period = (TimeSpan)batchingPeriod,
                EagerlyEmitFirstEvent = eagerlyEmitFirstEvent ?? DefaultEagerlyEmitFirstEvent,
                QueueLimit = queueSizeLimit ?? DefaultQueueSizeLimit
            };

            var batchingSink = new PeriodicBatchingSink(amazonS3Sink, batchingOptions);
            return sinkConfiguration.Sink(batchingSink, restrictedToMinimumLevel, levelSwitch);
        }

        /// <summary>Write log events to the specified file.</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one or more required arguments are
        /// null.
        /// </exception>
        /// <param name="sinkConfiguration">The logger sink configuration.</param>
        /// <param name="path">The path to the file</param>
        /// <param name="bucketName">The Amazon S3 bucket name.</param>
        /// <param name="serviceUrl">The Amazon S3 service url.</param>
        /// <param name="awsAccessKeyId">The Amazon S3 access key id.</param>
        /// <param name="awsSecretAccessKey">The Amazon S3 access key.</param>
        /// <param name="restrictedToMinimumLevel">
        /// (Optional)
        /// The minimum level for
        /// events passed through the sink. Ignored when
        /// <paramref name="levelSwitch" /> is specified.
        /// </param>
        /// <param name="outputTemplate">The output template.</param>
        /// <param name="formatProvider">
        /// (Optional)
        /// Supplies culture-specific formatting information, or
        /// null.
        /// </param>
        /// <param name="levelSwitch">
        /// (Optional)
        /// A switch allowing the pass-through minimum level
        /// to be changed at runtime.
        /// </param>
        /// <param name="rollingInterval">
        /// (Optional)
        /// The interval at which logging will roll over to a new
        /// file.
        /// </param>
        /// <param name="encoding">
        /// (Optional)
        /// Character encoding used to write the text file. The
        /// default is UTF-8 without BOM.
        /// </param>
        /// <param name="failureCallback">The failure callback.</param>
        /// <param name="bucketPath">The Amazon S3 bucket path.</param>
        /// <param name="batchSizeLimit">The batch size limit.</param>
        /// <param name="batchingPeriod">The batching period.</param>
        /// <param name="eagerlyEmitFirstEvent">A value indicating whether the first event should be emitted immediately or not.</param>
        /// <param name="queueSizeLimit">The queue size limit.</param>
        /// <returns>The configuration object allowing method chaining.</returns>
        public static LoggerConfiguration AmazonS3(
            this LoggerSinkConfiguration sinkConfiguration,
            string path,
            string bucketName,
            string serviceUrl,
            string awsAccessKeyId,
            string awsSecretAccessKey,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string outputTemplate = DefaultOutputTemplate,
            IFormatProvider formatProvider = null,
            LoggingLevelSwitch levelSwitch = null,
            RollingInterval rollingInterval = RollingInterval.Day,
            Encoding encoding = null,
            Action<Exception> failureCallback = null,
            string bucketPath = null,
            int? batchSizeLimit = DefaultBatchSizeLimit,
            TimeSpan? batchingPeriod = null,
            bool? eagerlyEmitFirstEvent = DefaultEagerlyEmitFirstEvent,
            int? queueSizeLimit = DefaultQueueSizeLimit)
        {
            if (sinkConfiguration is null)
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

            if (string.IsNullOrWhiteSpace(serviceUrl))
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

            if (string.IsNullOrWhiteSpace(outputTemplate))
            {
                outputTemplate = DefaultOutputTemplate;
            }

            if (encoding is null)
            {
                encoding = DefaultEncoding;
            }

            if (batchingPeriod is null)
            {
                batchingPeriod = DefaultBatchingPeriod;
            }

            var options = new AmazonS3Options
            {
                Path = path,
                BucketName = bucketName,
                ServiceUrl = serviceUrl,
                AwsAccessKeyId = awsAccessKeyId,
                AwsSecretAccessKey = awsSecretAccessKey,
                OutputTemplate = outputTemplate,
                FormatProvider = formatProvider,
                RollingInterval = rollingInterval,
                Encoding = encoding,
                FailureCallback = failureCallback,
                BucketPath = bucketPath
            };

            var amazonS3Sink = new AmazonS3Sink(options);

            var batchingOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = batchSizeLimit ?? DefaultBatchSizeLimit,
                Period = (TimeSpan)batchingPeriod,
                EagerlyEmitFirstEvent = eagerlyEmitFirstEvent ?? DefaultEagerlyEmitFirstEvent,
                QueueLimit = queueSizeLimit ?? DefaultQueueSizeLimit
            };

            var batchingSink = new PeriodicBatchingSink(amazonS3Sink, batchingOptions);
            return sinkConfiguration.Sink(batchingSink, restrictedToMinimumLevel, levelSwitch);
        }

        /// <summary>Write log events to the specified file.</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one or more required arguments are
        /// null.
        /// </exception>
        /// <param name="sinkConfiguration">The logger sink configuration.</param>
        /// <param name="path">The path to the file.</param>
        /// <param name="bucketName">The Amazon S3 bucket name.</param>
        /// <param name="serviceUrl">The Amazon S3 service url.</param>
        /// <param name="awsAccessKeyId">The Amazon S3 access key id.</param>
        /// <param name="awsSecretAccessKey">The Amazon S3 access key.</param>
        /// <param name="restrictedToMinimumLevel">
        /// (Optional)
        /// The minimum level for
        /// events passed through the sink. Ignored when
        /// <paramref name="levelSwitch" /> is specified.
        /// </param>
        /// <param name="levelSwitch">
        /// (Optional)
        /// A switch allowing the pass-through minimum level
        /// to be changed at runtime.
        /// </param>
        /// <param name="formatter">The formatter.</param>
        /// <param name="rollingInterval">
        /// (Optional)
        /// The interval at which logging will roll over to a new
        /// file.
        /// </param>
        /// <param name="encoding">
        /// (Optional)
        /// Character encoding used to write the text file. The
        /// default is UTF-8 without BOM.
        /// </param>
        /// <param name="failureCallback">The failure callback.</param>
        /// <param name="bucketPath">The Amazon S3 bucket path.</param>
        /// <param name="batchSizeLimit">The batch size limit.</param>
        /// <param name="batchingPeriod">The batching period.</param>
        /// <param name="eagerlyEmitFirstEvent">A value indicating whether the first event should be emitted immediately or not.</param>
        /// <param name="queueSizeLimit">The queue size limit.</param>
        /// <returns>The configuration object allowing method chaining.</returns>
        public static LoggerConfiguration AmazonS3(
                this LoggerSinkConfiguration sinkConfiguration,
                string path,
                string bucketName,
                string serviceUrl,
                string awsAccessKeyId,
                string awsSecretAccessKey,
                LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
                LoggingLevelSwitch levelSwitch = null,
                ITextFormatter formatter = null,
                RollingInterval rollingInterval = RollingInterval.Day,
                Encoding encoding = null,
                Action<Exception> failureCallback = null,
                string bucketPath = null,
                int? batchSizeLimit = DefaultBatchSizeLimit,
                TimeSpan? batchingPeriod = null,
                bool? eagerlyEmitFirstEvent = DefaultEagerlyEmitFirstEvent,
                int? queueSizeLimit = DefaultQueueSizeLimit)
        {
            if (sinkConfiguration is null)
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

            if (string.IsNullOrWhiteSpace(serviceUrl))
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

            if (formatter is null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (encoding is null)
            {
                encoding = DefaultEncoding;
            }

            if (batchingPeriod is null)
            {
                batchingPeriod = DefaultBatchingPeriod;
            }

            var options = new AmazonS3Options
            {
                Path = path,
                BucketName = bucketName,
                ServiceUrl = serviceUrl,
                AwsAccessKeyId = awsAccessKeyId,
                AwsSecretAccessKey = awsSecretAccessKey,
                Formatter = formatter,
                RollingInterval = rollingInterval,
                Encoding = encoding,
                FailureCallback = failureCallback,
                BucketPath = bucketPath
            };

            var amazonS3Sink = new AmazonS3Sink(options);

            var batchingOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = batchSizeLimit ?? DefaultBatchSizeLimit,
                Period = (TimeSpan)batchingPeriod,
                EagerlyEmitFirstEvent = eagerlyEmitFirstEvent ?? DefaultEagerlyEmitFirstEvent,
                QueueLimit = queueSizeLimit ?? DefaultQueueSizeLimit
            };

            var batchingSink = new PeriodicBatchingSink(amazonS3Sink, batchingOptions);
            return sinkConfiguration.Sink(batchingSink, restrictedToMinimumLevel, levelSwitch);
        }

        /// <summary>Write log events to the specified file.</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one or more required arguments are
        /// null.
        /// </exception>
        /// <param name="sinkConfiguration">The logger sink configuration.</param>
        /// <param name="path">The path to the file.</param>
        /// <param name="bucketName">The Amazon S3 bucket name.</param>
        /// <param name="serviceUrl">The Amazon S3 service url.</param>
        /// <param name="restrictedToMinimumLevel">
        /// (Optional)
        /// The minimum level for
        /// events passed through the sink. Ignored when
        /// <paramref name="levelSwitch" /> is specified.
        /// </param>
        /// <param name="levelSwitch">
        /// (Optional)
        /// A switch allowing the pass-through minimum level
        /// to be changed at runtime.
        /// </param>
        /// <param name="outputTemplate">
        /// (Optional)
        /// A message template describing the format used to
        /// write to the sink.
        /// The default is "{Timestamp:yyyy-MM-dd
        /// HH:mm:ss.fff zzz} [{Level:u3}]
        /// {Message:lj}{NewLine}{Exception}".
        /// </param>
        /// <param name="formatProvider">
        /// (Optional)
        /// Supplies culture-specific formatting information, or
        /// null.
        /// </param>
        /// <param name="rollingInterval">
        /// (Optional)
        /// The interval at which logging will roll over to a new
        /// file.
        /// </param>
        /// <param name="encoding">
        /// (Optional)
        /// Character encoding used to write the text file. The
        /// default is UTF-8 without BOM.
        /// </param>
        /// <param name="failureCallback">The failure callback.</param>
        /// <param name="bucketPath">The Amazon S3 bucket path.</param>
        /// <param name="batchSizeLimit">The batch size limit.</param>
        /// <param name="batchingPeriod">The batching period.</param>
        /// <param name="eagerlyEmitFirstEvent">A value indicating whether the first event should be emitted immediately or not.</param>
        /// <param name="queueSizeLimit">The queue size limit.</param>
        /// <returns>The configuration object allowing method chaining.</returns>
        public static LoggerConfiguration AmazonS3(
            this LoggerSinkConfiguration sinkConfiguration,
            string path,
            string bucketName,
            string serviceUrl,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch levelSwitch = null,
            string outputTemplate = DefaultOutputTemplate,
            IFormatProvider formatProvider = null,
            RollingInterval rollingInterval = RollingInterval.Day,
            Encoding encoding = null,
            Action<Exception> failureCallback = null,
            string bucketPath = null,
            int? batchSizeLimit = DefaultBatchSizeLimit,
            TimeSpan? batchingPeriod = null,
            bool? eagerlyEmitFirstEvent = DefaultEagerlyEmitFirstEvent,
            int? queueSizeLimit = DefaultQueueSizeLimit)
        {
            if (sinkConfiguration is null)
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

            if (string.IsNullOrWhiteSpace(serviceUrl))
            {
                throw new ArgumentNullException(nameof(serviceUrl));
            }

            if (string.IsNullOrWhiteSpace(outputTemplate))
            {
                outputTemplate = DefaultOutputTemplate;
            }

            if (encoding is null)
            {
                encoding = DefaultEncoding;
            }

            if (batchingPeriod is null)
            {
                batchingPeriod = DefaultBatchingPeriod;
            }

            var options = new AmazonS3Options
            {
                Path = path,
                BucketName = bucketName,
                ServiceUrl = serviceUrl,
                OutputTemplate = outputTemplate,
                FormatProvider = formatProvider,
                RollingInterval = rollingInterval,
                Encoding = encoding,
                FailureCallback = failureCallback,
                BucketPath = bucketPath
            };

            var amazonS3Sink = new AmazonS3Sink(options);

            var batchingOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = batchSizeLimit ?? DefaultBatchSizeLimit,
                Period = (TimeSpan)batchingPeriod,
                EagerlyEmitFirstEvent = eagerlyEmitFirstEvent ?? DefaultEagerlyEmitFirstEvent,
                QueueLimit = queueSizeLimit ?? DefaultQueueSizeLimit
            };

            var batchingSink = new PeriodicBatchingSink(amazonS3Sink, batchingOptions);
            return sinkConfiguration.Sink(batchingSink, restrictedToMinimumLevel, levelSwitch);
        }

        /// <summary>Write log events to the specified file.</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one or more required arguments are
        /// null.
        /// </exception>
        /// <param name="sinkConfiguration">The logger sink configuration.</param>
        /// <param name="path">The path to the file.</param>
        /// <param name="bucketName">The Amazon S3 bucket name.</param>
        /// <param name="serviceUrl">The Amazon S3 service url.</param>
        /// <param name="restrictedToMinimumLevel">
        /// (Optional)
        /// The minimum level for
        /// events passed through the sink. Ignored when
        /// <paramref name="levelSwitch" /> is specified.
        /// </param>
        /// <param name="levelSwitch">
        /// (Optional)
        /// A switch allowing the pass-through minimum level
        /// to be changed at runtime.
        /// </param>
        /// <param name="formatter">The formatter.</param>
        /// <param name="rollingInterval">
        /// (Optional)
        /// The interval at which logging will roll over to a new
        /// file.
        /// </param>
        /// <param name="encoding">
        /// (Optional)
        /// Character encoding used to write the text file. The
        /// default is UTF-8 without BOM.
        /// </param>
        /// <param name="failureCallback">The failure callback.</param>
        /// <param name="bucketPath">The Amazon S3 bucket path.</param>
        /// <param name="batchSizeLimit">The batch size limit.</param>
        /// <param name="batchingPeriod">The batching period.</param>
        /// <param name="eagerlyEmitFirstEvent">A value indicating whether the first event should be emitted immediately or not.</param>
        /// <param name="queueSizeLimit">The queue size limit.</param>
        /// <returns>The configuration object allowing method chaining.</returns>
        public static LoggerConfiguration AmazonS3(
            this LoggerSinkConfiguration sinkConfiguration,
            string path,
            string bucketName,
            string serviceUrl,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch levelSwitch = null,
            ITextFormatter formatter = null,
            RollingInterval rollingInterval = RollingInterval.Day,
            Encoding encoding = null,
            Action<Exception> failureCallback = null,
            string bucketPath = null,
            int? batchSizeLimit = DefaultBatchSizeLimit,
            TimeSpan? batchingPeriod = null,
            bool? eagerlyEmitFirstEvent = DefaultEagerlyEmitFirstEvent,
            int? queueSizeLimit = DefaultQueueSizeLimit)
        {
            if (sinkConfiguration is null)
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

            if (string.IsNullOrWhiteSpace(serviceUrl))
            {
                throw new ArgumentNullException(nameof(serviceUrl));
            }

            if (formatter is null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (encoding is null)
            {
                encoding = DefaultEncoding;
            }

            if (batchingPeriod is null)
            {
                batchingPeriod = DefaultBatchingPeriod;
            }

            var options = new AmazonS3Options
            {
                Path = path,
                BucketName = bucketName,
                ServiceUrl = serviceUrl,
                Formatter = formatter,
                RollingInterval = rollingInterval,
                Encoding = encoding,
                FailureCallback = failureCallback,
                BucketPath = bucketPath
            };

            var amazonS3Sink = new AmazonS3Sink(options);

            var batchingOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = batchSizeLimit ?? DefaultBatchSizeLimit,
                Period = (TimeSpan)batchingPeriod,
                EagerlyEmitFirstEvent = eagerlyEmitFirstEvent ?? DefaultEagerlyEmitFirstEvent,
                QueueLimit = queueSizeLimit ?? DefaultQueueSizeLimit
            };

            var batchingSink = new PeriodicBatchingSink(amazonS3Sink, batchingOptions);
            return sinkConfiguration.Sink(batchingSink, restrictedToMinimumLevel, levelSwitch);
        }

        /// <summary>Write log events to the specified file.</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one or more required arguments are
        /// null.
        /// </exception>
        /// <param name="sinkConfiguration">The logger sink configuration.</param>
        /// <param name="client">The Amazon S3 client.</param>
        /// <param name="path">The path to the file.</param>
        /// <param name="bucketName">The Amazon S3 bucket name.</param>
        /// <param name="restrictedToMinimumLevel">
        /// (Optional)
        /// The minimum level for
        /// events passed through the sink. Ignored when
        /// <paramref name="levelSwitch" /> is specified.
        /// </param>
        /// <param name="outputTemplate">
        /// (Optional)
        /// A message template describing the format used to
        /// write to the sink.
        /// The default is "{Timestamp:yyyy-MM-dd
        /// HH:mm:ss.fff zzz} [{Level:u3}]
        /// {Message:lj}{NewLine}{Exception}".
        /// </param>
        /// <param name="formatProvider">
        /// (Optional)
        /// Supplies culture-specific formatting information, or
        /// null.
        /// </param>
        /// <param name="levelSwitch">
        /// (Optional)
        /// A switch allowing the pass-through minimum level
        /// to be changed at runtime.
        /// </param>
        /// <param name="rollingInterval">
        /// (Optional)
        /// The interval at which logging will roll over to a new
        /// file.
        /// </param>
        /// <param name="encoding">
        /// (Optional)
        /// Character encoding used to write the text file. The
        /// default is UTF-8 without BOM.
        /// </param>
        /// <param name="failureCallback">The failure callback.</param>
        /// <param name="bucketPath">The Amazon S3 bucket path.</param>
        /// <param name="batchSizeLimit">The batch size limit.</param>
        /// <param name="batchingPeriod">The batching period.</param>
        /// <param name="eagerlyEmitFirstEvent">A value indicating whether the first event should be emitted immediately or not.</param>
        /// <param name="queueSizeLimit">The queue size limit.</param>
        /// <returns>The configuration object allowing method chaining.</returns>
        public static LoggerConfiguration AmazonS3(
            this LoggerSinkConfiguration sinkConfiguration,
            AmazonS3Client client,
            string path,
            string bucketName,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string outputTemplate = DefaultOutputTemplate,
            IFormatProvider formatProvider = null,
            LoggingLevelSwitch levelSwitch = null,
            RollingInterval rollingInterval = RollingInterval.Day,
            Encoding encoding = null,
            Action<Exception> failureCallback = null,
            string bucketPath = null,
            int? batchSizeLimit = DefaultBatchSizeLimit,
            TimeSpan? batchingPeriod = null,
            bool? eagerlyEmitFirstEvent = DefaultEagerlyEmitFirstEvent,
            int? queueSizeLimit = DefaultQueueSizeLimit)
        {
            if (sinkConfiguration is null)
            {
                throw new ArgumentNullException(nameof(sinkConfiguration));
            }

            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            if (string.IsNullOrWhiteSpace(outputTemplate))
            {
                outputTemplate = DefaultOutputTemplate;
            }

            if (encoding is null)
            {
                encoding = DefaultEncoding;
            }

            if (batchingPeriod is null)
            {
                batchingPeriod = DefaultBatchingPeriod;
            }

            var options = new AmazonS3Options
            {
                AmazonS3Client = client,
                Path = path,
                BucketName = bucketName,
                OutputTemplate = outputTemplate,
                FormatProvider = formatProvider,
                RollingInterval = rollingInterval,
                Encoding = encoding,
                FailureCallback = failureCallback,
                BucketPath = bucketPath
            };

            var amazonS3Sink = new AmazonS3Sink(options);

            var batchingOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = batchSizeLimit ?? DefaultBatchSizeLimit,
                Period = (TimeSpan)batchingPeriod,
                EagerlyEmitFirstEvent = eagerlyEmitFirstEvent ?? DefaultEagerlyEmitFirstEvent,
                QueueLimit = queueSizeLimit ?? DefaultQueueSizeLimit
            };

            var batchingSink = new PeriodicBatchingSink(amazonS3Sink, batchingOptions);
            return sinkConfiguration.Sink(batchingSink, restrictedToMinimumLevel, levelSwitch);
        }

        /// <summary>Write log events to the specified file.</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one or more required arguments are
        /// null.
        /// </exception>
        /// <param name="sinkConfiguration">The logger sink configuration.</param>
        /// <param name="client">The Amazon S3 client.</param>
        /// <param name="path">The path to the file.</param>
        /// <param name="bucketName">The Amazon S3 bucket name.</param>
        /// <param name="restrictedToMinimumLevel">
        /// (Optional)
        /// The minimum level for
        /// events passed through the sink. Ignored when
        /// <paramref name="levelSwitch" /> is specified.
        /// </param>
        /// <param name="levelSwitch">
        /// (Optional)
        /// A switch allowing the pass-through minimum level
        /// to be changed at runtime.
        /// </param>
        /// <param name="formatter">The formatter.</param>
        /// <param name="rollingInterval">
        /// (Optional)
        /// The interval at which logging will roll over to a new
        /// file.
        /// </param>
        /// <param name="encoding">
        /// (Optional)
        /// Character encoding used to write the text file. The
        /// default is UTF-8 without BOM.
        /// </param>
        /// <param name="failureCallback">The failure callback.</param>
        /// <param name="bucketPath">The Amazon S3 bucket path.</param>
        /// <param name="batchSizeLimit">The batch size limit.</param>
        /// <param name="batchingPeriod">The batching period.</param>
        /// <param name="eagerlyEmitFirstEvent">A value indicating whether the first event should be emitted immediately or not.</param>
        /// <param name="queueSizeLimit">The queue size limit.</param>
        /// <returns>The configuration object allowing method chaining.</returns>
        public static LoggerConfiguration AmazonS3(
            this LoggerSinkConfiguration sinkConfiguration,
            AmazonS3Client client,
            string path,
            string bucketName,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch levelSwitch = null,
            ITextFormatter formatter = null,
            RollingInterval rollingInterval = RollingInterval.Day,
            Encoding encoding = null,
            Action<Exception> failureCallback = null,
            string bucketPath = null,
            int? batchSizeLimit = DefaultBatchSizeLimit,
            TimeSpan? batchingPeriod = null,
            bool? eagerlyEmitFirstEvent = DefaultEagerlyEmitFirstEvent,
            int? queueSizeLimit = DefaultQueueSizeLimit)
        {
            if (sinkConfiguration is null)
            {
                throw new ArgumentNullException(nameof(sinkConfiguration));
            }

            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentNullException(nameof(bucketName));
            }

            if (formatter is null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (encoding is null)
            {
                encoding = DefaultEncoding;
            }

            if (batchingPeriod is null)
            {
                batchingPeriod = DefaultBatchingPeriod;
            }

            var options = new AmazonS3Options
            {
                AmazonS3Client = client,
                Path = path,
                BucketName = bucketName,
                Formatter = formatter,
                RollingInterval = rollingInterval,
                Encoding = encoding,
                FailureCallback = failureCallback,
                BucketPath = bucketPath
            };

            var amazonS3Sink = new AmazonS3Sink(options);

            var batchingOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = batchSizeLimit ?? DefaultBatchSizeLimit,
                Period = (TimeSpan)batchingPeriod,
                EagerlyEmitFirstEvent = eagerlyEmitFirstEvent ?? DefaultEagerlyEmitFirstEvent,
                QueueLimit = queueSizeLimit ?? DefaultQueueSizeLimit
            };

            var batchingSink = new PeriodicBatchingSink(amazonS3Sink, batchingOptions);
            return sinkConfiguration.Sink(batchingSink, restrictedToMinimumLevel, levelSwitch);
        }
    }
}