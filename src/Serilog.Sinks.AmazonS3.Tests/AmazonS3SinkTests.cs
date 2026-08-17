// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AmazonS3SinkTests.cs" company="SeppPenner and the Serilog contributors">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//   This class contains the tests for the <see cref="AmazonS3Sink"/> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3.Tests;

/// <summary>
/// This class contains the tests for the <see cref="AmazonS3Sink"/> class. They run against a fake Amazon S3
/// client, so nothing leaves the machine and no credentials are needed.
/// </summary>
[TestClass]
public class AmazonS3SinkTests
{
    /// <summary>
    /// An Amazon S3 client that captures the upload instead of sending it.
    /// </summary>
    private sealed class FakeAmazonS3Client : AmazonS3Client
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FakeAmazonS3Client"/> class.
        /// </summary>
        public FakeAmazonS3Client()
            : base(new BasicAWSCredentials("key", "secret"), new AmazonS3Config { ServiceURL = "https://localhost" })
        {
        }

        /// <summary>
        /// Gets the keys of all uploads in the order they arrived.
        /// </summary>
        public List<string> Keys { get; } = [];

        /// <summary>
        /// Gets the bucket name of the last upload.
        /// </summary>
        public string BucketName { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the bytes of the last upload.
        /// </summary>
        public byte[] Bytes { get; private set; } = [];

        /// <summary>
        /// Gets the content of the last upload. A byte order mark is kept, it shows up as the first character.
        /// </summary>
        public string Content => Encoding.UTF8.GetString(this.Bytes);

        /// <summary>
        /// Captures the request instead of sending it to Amazon S3.
        /// </summary>
        /// <param name="request">The put object request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An empty <see cref="PutObjectResponse"/>.</returns>
        public override Task<PutObjectResponse> PutObjectAsync(PutObjectRequest request, CancellationToken cancellationToken = default)
        {
            this.BucketName = request.BucketName;
            this.Keys.Add(request.Key);

            using var memoryStream = new MemoryStream();
            request.InputStream.CopyTo(memoryStream);
            this.Bytes = memoryStream.ToArray();

            return Task.FromResult(new PutObjectResponse());
        }
    }

    /// <summary>
    /// This method is used to test that a batch is formatted, uploaded and removed from the local disk.
    /// </summary>
    /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
    [TestMethod]
    public async Task ABatchIsUploadedAndTheLocalFileIsDeleted()
    {
        var directory = CreateTestDirectory();

        try
        {
            using var client = new FakeAmazonS3Client();
            using var sink = new AmazonS3Sink(CreateOptions(client, directory, null));

            await sink.EmitBatchAsync([CreateLogEvent("Hello Amazon S3")]);

            Assert.AreEqual("mytestbucket-aws", client.BucketName);
            Assert.AreEqual($"log{DateTime.Now:yyyyMMdd}.txt", client.Keys.Single());
            StringAssert.Contains(client.Content, "Hello Amazon S3");
            Assert.AreEqual(0, Directory.GetFiles(directory).Length, "The uploaded file was not deleted.");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    /// <summary>
    /// This method is used to test that every batch after the first one gets its own sequence number.
    /// </summary>
    /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
    [TestMethod]
    public async Task EveryBatchGetsItsOwnFile()
    {
        var directory = CreateTestDirectory();

        try
        {
            using var client = new FakeAmazonS3Client();
            using var sink = new AmazonS3Sink(CreateOptions(client, directory, null));

            await sink.EmitBatchAsync([CreateLogEvent("First")]);
            await sink.EmitBatchAsync([CreateLogEvent("Second")]);
            await sink.EmitBatchAsync([CreateLogEvent("Third")]);

            var today = $"{DateTime.Now:yyyyMMdd}";
            CollectionAssert.AreEqual(
                new List<string> { $"log{today}.txt", $"log{today}_001.txt", $"log{today}_002.txt" },
                client.Keys);
            StringAssert.Contains(client.Content, "Third");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    /// <summary>
    /// This method is used to test that the bucket path is put in front of the key.
    /// </summary>
    /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
    [TestMethod]
    public async Task TheBucketPathIsPutInFrontOfTheKey()
    {
        var directory = CreateTestDirectory();

        try
        {
            using var client = new FakeAmazonS3Client();
            using var sink = new AmazonS3Sink(CreateOptions(client, directory, "awsSubPath"));

            await sink.EmitBatchAsync([CreateLogEvent("Hello Amazon S3")]);

            Assert.AreEqual($"awsSubPath/log{DateTime.Now:yyyyMMdd}.txt", client.Keys.Single());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    /// <summary>
    /// This method is used to test that an empty file is refused instead of being uploaded.
    /// </summary>
    /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
    [TestMethod]
    public async Task AnEmptyFileIsNotUploaded()
    {
        var directory = CreateTestDirectory();

        try
        {
            using var client = new FakeAmazonS3Client();
            using var sink = new AmazonS3Sink(CreateOptions(client, directory, null));

            await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
            {
                await sink.EmitBatchAsync([]);
            });

            Assert.AreEqual(0, client.Keys.Count);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    /// <summary>
    /// This method is used to test that the uploaded file starts with the first event and not with a byte order mark.
    /// The encoding parameter of the sink is documented as UTF-8 without BOM.
    /// </summary>
    /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
    [TestMethod]
    public async Task TheUploadedFileHasNoByteOrderMark()
    {
        var directory = CreateTestDirectory();

        try
        {
            using var client = new FakeAmazonS3Client();
            using var sink = new AmazonS3Sink(CreateOptions(client, directory, null));

            await sink.EmitBatchAsync([CreateLogEvent("Hello Amazon S3")]);

            StringAssert.StartsWith(client.Content, "Hello Amazon S3");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    /// <summary>
    /// Creates an empty directory below the temporary directory for a single test.
    /// </summary>
    /// <returns>The path of the created directory.</returns>
    private static string CreateTestDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"Serilog.Sinks.AmazonS3.Tests.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return directory;
    }

    /// <summary>
    /// Creates the options for a sink that writes into the given directory and uploads with the given client.
    /// </summary>
    /// <param name="client">The Amazon S3 client to use.</param>
    /// <param name="directory">The directory the local files are written to.</param>
    /// <param name="bucketPath">The bucket path or <c>null</c> for none.</param>
    /// <returns>The <see cref="AmazonS3Options"/>.</returns>
    private static AmazonS3Options CreateOptions(AmazonS3Client client, string directory, string? bucketPath)
    {
        return new AmazonS3Options
        {
            AmazonS3Client = client,
            BucketName = "mytestbucket-aws",
            BucketPath = bucketPath,
            Path = Path.Combine(directory, "log.txt"),
            RollingInterval = RollingInterval.Day,
            OutputTemplate = "{Message:lj}{NewLine}"
        };
    }

    /// <summary>
    /// Creates a log event with the given message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The <see cref="LogEvent"/>.</returns>
    private static LogEvent CreateLogEvent(string message)
    {
        var messageTemplate = new MessageTemplateParser().Parse(message);
        return new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, messageTemplate, []);
    }
}
