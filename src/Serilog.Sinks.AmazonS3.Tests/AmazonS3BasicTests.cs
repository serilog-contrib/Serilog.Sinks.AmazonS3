// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AmazonS3BasicTests.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class is used for some basic test regarding the Amazon S3 sink.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3.Tests;

/// <summary>
/// This class is used for some basic test regarding the Amazon S3 sink.
/// </summary>
[TestClass]
public class AmazonS3BasicTests
{
    /// <summary>
    /// The Amazon S3 access key id.
    /// </summary>
    private readonly string awsAccessKeyId = Environment.GetEnvironmentVariable("AwsAccessKeyId") ?? string.Empty;

    /// <summary>
    /// The Amazon S3 secret access key.
    /// </summary>
    private readonly string awsSecretAccessKey = Environment.GetEnvironmentVariable("AwsSecretAccessKey") ?? string.Empty;

    /// <summary>
    /// The Amazon S3 bucket name.
    /// </summary>
    private readonly string awsBucketName = Environment.GetEnvironmentVariable("AwsBucketName") ?? string.Empty;

    /// <summary>
    /// Initializes global settings for the current test class.
    /// </summary>
    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        SelfLog.Enable(Console.WriteLine);
    }

    /// <summary>
    /// This method is used to test a basic file upload to Amazon S3 with no credentials (IAM).
    /// </summary>
    [TestMethod]
    public void BasicFileUploadAuthorizedTest()
    {
        var logger = new LoggerConfiguration()
            .WriteTo.AmazonS3(
                "log.txt",
                this.awsBucketName,
                RegionEndpoint.EUWest2,
                LogEventLevel.Verbose,
                outputTemplate: null,
                formatProvider: null,
                levelSwitch: null,
                rollingInterval: RollingInterval.Minute,
                encoding: null,
                bucketPath: null,
                batchSizeLimit: null,
                batchingPeriod: null,
                eagerlyEmitFirstEvent: null,
                queueSizeLimit: null,
                disablePayloadSigning: null)
            .CreateLogger();

        for (var x = 0; x < 200; x++)
        {
            var ex = new Exception("Test");
#pragma warning disable Serilog004 // Constant MessageTemplate verifier
            logger.Error(ex.ToString());
#pragma warning restore Serilog004 // Constant MessageTemplate verifier
        }

        Log.CloseAndFlush();
    }

    /// <summary>
    /// This method is used to test a basic file upload to Amazon S3.
    /// </summary>
    [TestMethod]
    public void BasicFileUploadTest()
    {
        var logger = new LoggerConfiguration()
            .WriteTo.AmazonS3(
                "log.txt",
                this.awsBucketName,
                RegionEndpoint.EUWest2,
                this.awsAccessKeyId,
                this.awsSecretAccessKey,
                LogEventLevel.Verbose,
                outputTemplate: null,
                formatProvider: null,
                levelSwitch: null,
                rollingInterval: RollingInterval.Minute,
                encoding: null,
                bucketPath: null,
                batchSizeLimit: null,
                batchingPeriod: null,
                eagerlyEmitFirstEvent: null,
                queueSizeLimit: null,
                disablePayloadSigning: null)
            .CreateLogger();

        for (var x = 0; x < 200; x++)
        {
            var ex = new Exception("Test");
#pragma warning disable Serilog004 // Constant MessageTemplate verifier
            logger.Error(ex.ToString());
#pragma warning restore Serilog004 // Constant MessageTemplate verifier
        }

        Log.CloseAndFlush();
    }

    /// <summary>
    ///     This method is used to test the JSON upload functionality.
    /// </summary>
    [TestMethod]
    public void JsonFileUploadTest()
    {
        var logger = new LoggerConfiguration()
            .WriteTo.AmazonS3(
                "log.txt",
                this.awsBucketName,
                RegionEndpoint.EUWest2,
                this.awsAccessKeyId,
                this.awsSecretAccessKey,
                LogEventLevel.Verbose,
                formatter: new CompactJsonFormatter(),
                levelSwitch: null,
                rollingInterval: RollingInterval.Minute,
                encoding: null,
                bucketPath: null,
                batchSizeLimit: null,
                batchingPeriod: null,
                eagerlyEmitFirstEvent: null,
                queueSizeLimit: null,
                disablePayloadSigning: null)
            .CreateLogger();

        for (var x = 0; x < 200; x++)
        {
            var ex = new Exception("Test");
#pragma warning disable Serilog004 // Constant MessageTemplate verifier
            logger.Error(ex.ToString());
#pragma warning restore Serilog004 // Constant MessageTemplate verifier
        }

        Log.CloseAndFlush();
    }

    /// <summary>
    ///     This method is used to test the formatting functionality.
    /// </summary>
    [TestMethod]
    public void FormattingTest()
    {
        var logger = new LoggerConfiguration()
            .WriteTo.AmazonS3(
                "log.txt",
                this.awsBucketName,
                RegionEndpoint.EUWest2,
                this.awsAccessKeyId,
                this.awsSecretAccessKey,
                LogEventLevel.Verbose,
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        for (var x = 0; x < 200; x++)
        {
            var ex = new Exception("Test");
#pragma warning disable Serilog004 // Constant MessageTemplate verifier
            logger.Error(ex.ToString());
#pragma warning restore Serilog004 // Constant MessageTemplate verifier
        }

        Log.CloseAndFlush();
    }

    /// <summary>
    /// This method is used to test a basic file upload to Amazon S3 with the settings provided from https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/issues/52
    /// that tests the rolling interval.
    /// </summary>
    [TestMethod]
    public void BasicFileUploadRollingIntervalTest()
    {
        var logger = new LoggerConfiguration()
            .WriteTo.AmazonS3(
                restrictedToMinimumLevel: LogEventLevel.Debug,
                path: "log.txt",
                bucketName: this.awsBucketName,
                endpoint: RegionEndpoint.EUWest2,
                bucketPath: null,
                awsAccessKeyId: this.awsAccessKeyId,
                awsSecretAccessKey: this.awsSecretAccessKey,
                rollingInterval: RollingInterval.Hour,
                eagerlyEmitFirstEvent: false,
                formatter: new JsonFormatter(),
                batchingPeriod: TimeSpan.FromHours(2),
                batchSizeLimit: 100)
            .CreateLogger();

        for (var x = 0; x < 200; x++)
        {
            var ex = new Exception("Test");
#pragma warning disable Serilog004 // Constant MessageTemplate verifier
            logger.Error(ex.ToString());
#pragma warning restore Serilog004 // Constant MessageTemplate verifier
        }

        Log.CloseAndFlush();
    }
}
