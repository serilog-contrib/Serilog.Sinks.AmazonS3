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

    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Formatting.Compact;

    /// <summary>   This class is used for some basic test regarding the Amazon S3 sink. </summary>
    [TestClass]
    public class AmazonS3BasicTests
    {
        /// <summary>   The Amazon S3 access key id. </summary>
        private const string AwsAccessKeyId = "yourAccessKeyId";

        /// <summary>   The Amazon S3 secret access key. </summary>
        private const string AwsSecretAccessKey = "yourSecretAccessKey";

        /// <summary>   The Amazon S3 bucket name. </summary>
        private const string BucketName = "your-test-bucket";

        /// <summary>   This method is used to test a basic file upload to Amazon S3. </summary>
        [TestMethod]
        public void BasicFileUploadAuthorizedTest()
        {
            var logger = new LoggerConfiguration().WriteTo.AmazonS3(
                "log.txt",
                BucketName,
                RegionEndpoint.EUWest2,
                fileSizeLimitBytes: 200,
                rollingInterval: RollingInterval.Minute,
                failureCallback: e => Console.WriteLine($"Sink error: {e.Message}")).CreateLogger();

            for (var x = 0; x < 200; x++)
            {
                var ex = new Exception("Test");
                logger.Error(ex.ToString());
            }
        }

        /// <summary>   This method is used to test a basic file upload to Amazon S3. </summary>
        [TestMethod]
        public void BasicFileUploadTest()
        {
            var logger = new LoggerConfiguration().WriteTo.AmazonS3(
                "log.txt",
                BucketName,
                RegionEndpoint.EUWest2,
                AwsAccessKeyId,
                AwsSecretAccessKey,
                fileSizeLimitBytes: 200,
                rollingInterval: RollingInterval.Minute).CreateLogger();

            for (var x = 0; x < 200; x++)
            {
                var ex = new Exception("Test");
                logger.Error(ex.ToString());
            }
        }

        /// <summary>
        ///     This method is used to test find the null reference exception of issue 8.
        /// </summary>
        [TestMethod]
        public void ExtendedFileUploadTest()
        {
            var levelSwitch = new LoggingLevelSwitch { MinimumLevel = LogEventLevel.Information };

            var logger = new LoggerConfiguration()
                .WriteTo.AmazonS3(
                    "log.txt",
                    BucketName,
                    RegionEndpoint.EUWest2,
                    AwsAccessKeyId,
                    AwsSecretAccessKey,
                    restrictedToMinimumLevel: LogEventLevel.Verbose,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    fileSizeLimitBytes: 200,
                    levelSwitch: levelSwitch,
                    buffered: true,
                    rollingInterval: RollingInterval.Minute,
                    retainedFileCountLimit: 10,
                    autoUploadEvents: true)
                .CreateLogger();

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
            var levelSwitch = new LoggingLevelSwitch { MinimumLevel = LogEventLevel.Information };

            var logger = new LoggerConfiguration()
                .WriteTo.AmazonS3(
                    new CompactJsonFormatter(),
                    "log.json",
                    BucketName,
                    RegionEndpoint.EUWest2,
                    AwsAccessKeyId,
                    AwsSecretAccessKey,
                    restrictedToMinimumLevel: LogEventLevel.Verbose,
                    fileSizeLimitBytes: 200,
                    levelSwitch: levelSwitch,
                    buffered: true,
                    rollingInterval: RollingInterval.Minute,
                    retainedFileCountLimit: 10,
                    autoUploadEvents: true)
                .CreateLogger();

            for (var x = 0; x < 200; x++)
            {
                var ex = new Exception("Test");
                logger.Error(ex.ToString());
            }
        }
    }
}