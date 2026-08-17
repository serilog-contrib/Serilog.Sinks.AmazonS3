// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoggerConfigurationAmazonS3ExtensionsTests.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class contains the tests for the argument checks of the Amazon S3 logger configuration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3.Tests;

/// <summary>
/// This class contains the tests for the argument checks of the Amazon S3 logger configuration.
/// These tests never talk to Amazon S3, they only configure a logger and check what it refuses.
/// </summary>
[TestClass]
public class LoggerConfigurationAmazonS3ExtensionsTests
{
    /// <summary>
    /// The output template used to pick the overload without a text formatter.
    /// </summary>
    private const string OutputTemplate = "{Message:lj}{NewLine}";

    /// <summary>
    /// This method is used to test that an empty path is refused.
    /// </summary>
    [TestMethod]
    public void AnEmptyPathIsRefused()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            _ = new LoggerConfiguration().WriteTo.AmazonS3(
                string.Empty,
                "mytestbucket-aws",
                RegionEndpoint.EUWest2,
                outputTemplate: OutputTemplate);
        });
    }

    /// <summary>
    /// This method is used to test that an empty bucket name is refused.
    /// </summary>
    [TestMethod]
    public void AnEmptyBucketNameIsRefused()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            _ = new LoggerConfiguration().WriteTo.AmazonS3(
                "log.txt",
                string.Empty,
                RegionEndpoint.EUWest2,
                outputTemplate: OutputTemplate);
        });
    }

    /// <summary>
    /// This method is used to test that a missing text formatter is refused by the overloads that expect one.
    /// </summary>
    [TestMethod]
    public void AMissingFormatterIsRefused()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            _ = new LoggerConfiguration().WriteTo.AmazonS3(
                "log.txt",
                "mytestbucket-aws",
                RegionEndpoint.EUWest2,
                formatter: null);
        });
    }

    /// <summary>
    /// This method is used to test that a missing Amazon S3 client is refused by the overloads that expect one.
    /// </summary>
    [TestMethod]
    public void AMissingClientIsRefused()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            _ = new LoggerConfiguration().WriteTo.AmazonS3(
                client: null!,
                path: "log.txt",
                bucketName: "mytestbucket-aws",
                outputTemplate: OutputTemplate);
        });
    }

    /// <summary>
    /// This method is used to test that a service url that is not an absolute url is refused.
    /// </summary>
    [TestMethod]
    public void ARelativeServiceUrlIsRefused()
    {
        var exception = Assert.ThrowsExactly<ArgumentException>(() =>
        {
            _ = new LoggerConfiguration().WriteTo.AmazonS3(
                "log.txt",
                "mytestbucket-aws",
                "s3.eu-west-2.amazonaws.com",
                outputTemplate: OutputTemplate);
        });

        Assert.AreEqual("serviceUrl", exception.ParamName);
    }

    /// <summary>
    /// This method is used to test that an absolute service url is accepted. Nothing is uploaded because
    /// no event is written, the logger is only built and disposed again.
    /// </summary>
    [TestMethod]
    public void AnAbsoluteServiceUrlIsAccepted()
    {
        using var logger = new LoggerConfiguration().WriteTo.AmazonS3(
            "log.txt",
            "mytestbucket-aws",
            "https://s3.eu-west-2.amazonaws.com",
            outputTemplate: OutputTemplate)
            .CreateLogger();

        Assert.IsNotNull(logger);
    }
}
