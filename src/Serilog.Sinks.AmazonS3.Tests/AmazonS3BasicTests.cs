// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AmazonS3BasicTests.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used for some basic test regarding the Amazon S3 sink.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3.Tests
{
    using System;

    using Amazon;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Serilog.Events;
    using Serilog.Formatting.Compact;

    /// <summary>
    /// This class is used for some basic test regarding the Amazon S3 sink.
    /// </summary>
    [TestClass]
    public class AmazonS3BasicTests
    {
        /// <summary>
        /// The Amazon S3 access key id.
        /// </summary>
        private const string AwsAccessKeyId = "Dummy";

        /// <summary>
        /// The Amazon S3 secret access key.
        /// </summary>
        private const string AwsSecretAccessKey = "Dummy";

        /// <summary>
        /// The Amazon S3 bucket name.
        /// </summary>
        private const string BucketName = "dummy";

        /// <summary>
        /// This method is used to test a basic file upload to Amazon S3 with no credentials (IAM).
        /// </summary>
        [TestMethod]
        public void BasicFileUploadAuthorizedTest()
        {
            var logger = new LoggerConfiguration().WriteTo.AmazonS3(
                "log.txt",
                BucketName,
                RegionEndpoint.EUWest2,
                LogEventLevel.Verbose,
                outputTemplate: null,
                formatProvider: null,
                levelSwitch: null,
                rollingInterval: RollingInterval.Minute,
                encoding: null,
                failureCallback: e => Console.WriteLine($"Sink error: {e.Message}, {e.StackTrace}"),
                bucketPath: null,
                batchSizeLimit: null,
                batchingPeriod: null,
                eagerlyEmitFirstEvent: null,
                queueSizeLimit: null).CreateLogger();

            for (var x = 0; x < 200; x++)
            {
                var ex = new Exception("Test");
                logger.Error(ex.ToString());
            }
        }

        /// <summary>
        /// This method is used to test a basic file upload to Amazon S3.
        /// </summary>
        // ReSharper disable ArgumentsStyleLiteral
        // ReSharper disable ArgumentsStyleNamedExpression
        [TestMethod]
        public void BasicFileUploadTest()
        {
            var logger = new LoggerConfiguration().WriteTo.AmazonS3(
                "log.txt",
                BucketName,
                RegionEndpoint.EUWest2,
                AwsAccessKeyId,
                AwsSecretAccessKey,
                LogEventLevel.Verbose,
                outputTemplate: null,
                formatProvider: null,
                levelSwitch: null,
                rollingInterval: RollingInterval.Minute,
                encoding: null,
                failureCallback: e => Console.WriteLine($"Sink error: {e.Message}, {e.StackTrace}"),
                bucketPath: null,
                batchSizeLimit: null,
                batchingPeriod: null,
                eagerlyEmitFirstEvent: null,
                queueSizeLimit: null).CreateLogger();

            for (var x = 0; x < 200; x++)
            {
                var ex = new Exception("Test");
                logger.Error(ex.ToString());
            }
        }

        /// <summary>
        ///     This method is used to test the JSON upload functionality.
        /// </summary>
        [TestMethod]
        public void JsonFileUploadTest()
        {
            var logger = new LoggerConfiguration().WriteTo.AmazonS3(
                "log.txt",
                BucketName,
                RegionEndpoint.EUWest2,
                AwsAccessKeyId,
                AwsSecretAccessKey,
                LogEventLevel.Verbose,
                formatter: new CompactJsonFormatter(),
                levelSwitch: null,
                rollingInterval: RollingInterval.Minute,
                encoding: null,
                failureCallback: e => Console.WriteLine($"Sink error: {e.Message}, {e.StackTrace}"),
                bucketPath: null,
                batchSizeLimit: null,
                batchingPeriod: null,
                eagerlyEmitFirstEvent: null,
                queueSizeLimit: null).CreateLogger();

            for (var x = 0; x < 200; x++)
            {
                var ex = new Exception("Test");
                logger.Error(ex.ToString());
            }
        }
    }
}