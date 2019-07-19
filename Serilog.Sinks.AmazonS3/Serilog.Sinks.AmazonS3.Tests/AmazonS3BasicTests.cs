// file:	AmazonS3BasicTests.cs
//
// summary:	Implements the amazon s 3 basic tests class

namespace Serilog.Sinks.AmazonS3.Tests
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Serilog;

    /// <summary>   This class is used for some basic test regarding the Amazon S3 sink. </summary>
    [TestClass]
    public class AmazonS3BasicTests
    {
        /// <summary>   The Amazon S3 bucket name. </summary>
        private const string BucketName = "your-test-bucket";

        /// <summary>   The Amazon S3 access key id. </summary>
        private const string AwsAccessKeyId = "yourAccessKeyId";

        /// <summary>   The Amazon S3 secret access key. </summary>
        private const string AwsSecretAccessKey = "yourSecretAccessKey";

        /// <summary>   This method is used to test a basic file upload to Amazon S3. </summary>
        [TestMethod]
        public void BasicFileUploadTest()
        {
            var logger = new LoggerConfiguration().WriteTo
                .AmazonS3(
                    "log.txt",
                    BucketName,
                    Amazon.RegionEndpoint.EUWest2,
                    AwsAccessKeyId,
                    AwsSecretAccessKey,
                    fileSizeLimitBytes: 200,
                    rollingInterval: RollingInterval.Minute)
                .CreateLogger();

            for (var x = 0; x < 200; x++)
            {
                var ex = new Exception("Test");
                logger.Error(ex.ToString());
            }
        }

        /// <summary>   This method is used to test a basic file upload to Amazon S3. </summary>
        [TestMethod]
        public void BasicFileUploadAuthorizedTest()
        {
            var logger = new LoggerConfiguration().WriteTo
                .AmazonS3(
                    "log.txt",
                    BucketName,
                    Amazon.RegionEndpoint.EUWest2,
                    fileSizeLimitBytes: 200,
                    rollingInterval: RollingInterval.Minute)
                .CreateLogger();

            for (var x = 0; x < 200; x++)
            {
                var ex = new Exception("Test");
                logger.Error(ex.ToString());
            }
        }
    }
}