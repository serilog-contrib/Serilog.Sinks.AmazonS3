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
/// client and a fake clock, so nothing leaves the machine, no credentials are needed and the rolling intervals
/// pass at once.
/// </summary>
[TestClass]
public class AmazonS3SinkTests
{
    /// <summary>
    /// The time every test starts at, a quarter past ten. With <see cref="RollingInterval.Hour"/> the interval ends at
    /// eleven and its file is called <c>log2026112410.txt</c>.
    /// </summary>
    private static readonly DateTimeOffset Start = new(2026, 11, 24, 10, 15, 0, TimeSpan.Zero);

    /// <summary>
    /// An Amazon S3 client that keeps the uploads in memory instead of sending them.
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
        /// Gets the content of every object in the fake bucket by key.
        /// </summary>
        public Dictionary<string, string> Objects { get; } = [];

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
        /// Gets or sets a value indicating whether the next upload fails.
        /// </summary>
        public bool FailNextUpload { get; set; }

        /// <summary>
        /// Keeps the request in memory instead of sending it to Amazon S3.
        /// </summary>
        /// <param name="request">The put object request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An empty <see cref="PutObjectResponse"/>.</returns>
        public override Task<PutObjectResponse> PutObjectAsync(PutObjectRequest request, CancellationToken cancellationToken = default)
        {
            if (this.FailNextUpload)
            {
                this.FailNextUpload = false;
                throw new AmazonS3Exception("The fake upload failed.");
            }

            this.BucketName = request.BucketName;
            this.Keys.Add(request.Key);

            using var memoryStream = new MemoryStream();
            request.InputStream.CopyTo(memoryStream);
            this.Bytes = memoryStream.ToArray();
            this.Objects[request.Key] = this.Content;

            return Task.FromResult(new PutObjectResponse());
        }

        /// <summary>
        /// Answers whether the key is taken in the fake bucket, a missing key throws like Amazon S3 does.
        /// </summary>
        /// <param name="request">The get object metadata request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An empty <see cref="GetObjectMetadataResponse"/>.</returns>
        public override Task<GetObjectMetadataResponse> GetObjectMetadataAsync(GetObjectMetadataRequest request, CancellationToken cancellationToken = default)
        {
            if (this.Objects.ContainsKey(request.Key))
            {
                return Task.FromResult(new GetObjectMetadataResponse());
            }

            throw new AmazonS3Exception("Not Found", ErrorType.Sender, "NotFound", "FakeRequestId", HttpStatusCode.NotFound);
        }
    }

    /// <summary>
    /// A clock that only moves when a test sets it. Local time is UTC, so the file names do not depend on the machine.
    /// </summary>
    private sealed class FakeTimeProvider : TimeProvider
    {
        /// <summary>
        /// Gets or sets the current time.
        /// </summary>
        public DateTimeOffset UtcNow { get; set; } = Start;

        /// <summary>
        /// Gets the local time zone, which is UTC here.
        /// </summary>
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

        /// <summary>
        /// Gets the current time.
        /// </summary>
        /// <returns>The current time as <see cref="DateTimeOffset"/>.</returns>
        public override DateTimeOffset GetUtcNow() => this.UtcNow;
    }

    /// <summary>
    /// This method is used to test that all batches of a rolling interval end up in one file that is uploaded once the
    /// interval is over, even when no further event arrives. See
    /// https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/issues/52.
    /// </summary>
    /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
    [TestMethod]
    public async Task TheBatchesOfAnIntervalAreUploadedAsOneFileWhenTheIntervalIsOver()
    {
        var directory = CreateTestDirectory();

        try
        {
            using var client = new FakeAmazonS3Client();
            var clock = new FakeTimeProvider();
            using var sink = CreateSink(client, directory, clock);

            await sink.EmitBatchAsync([CreateLogEvent("First")]);
            clock.UtcNow = Start.AddMinutes(15);
            await sink.EmitBatchAsync([CreateLogEvent("Second")]);
            clock.UtcNow = Start.AddMinutes(44);
            await sink.OnEmptyBatchAsync();

            Assert.AreEqual(0, client.Keys.Count, "Nothing may be uploaded before the interval is over.");

            clock.UtcNow = Start.AddMinutes(46);
            await sink.OnEmptyBatchAsync();

            Assert.AreEqual("mytestbucket-aws", client.BucketName);
            Assert.AreEqual("log2026112410.txt", client.Keys.Single());
            Assert.AreEqual($"First{Environment.NewLine}Second{Environment.NewLine}", client.Content);
            Assert.AreEqual(0, Directory.GetFiles(directory).Length, "The uploaded file was not deleted.");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    /// <summary>
    /// This method is used to test that the first batch of a new rolling interval goes into a new file and uploads the
    /// file of the interval before.
    /// </summary>
    /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
    [TestMethod]
    public async Task EveryIntervalGetsItsOwnFile()
    {
        var directory = CreateTestDirectory();

        try
        {
            using var client = new FakeAmazonS3Client();
            var clock = new FakeTimeProvider();
            using var sink = CreateSink(client, directory, clock);

            await sink.EmitBatchAsync([CreateLogEvent("Ten")]);
            clock.UtcNow = Start.AddHours(1);
            await sink.EmitBatchAsync([CreateLogEvent("Eleven")]);

            Assert.AreEqual("log2026112410.txt", client.Keys.Single());
            Assert.AreEqual($"Ten{Environment.NewLine}", client.Content);

            clock.UtcNow = Start.AddHours(2);
            await sink.OnEmptyBatchAsync();

            CollectionAssert.AreEqual(new List<string> { "log2026112410.txt", "log2026112411.txt" }, client.Keys);
            Assert.AreEqual($"Eleven{Environment.NewLine}", client.Content);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    /// <summary>
    /// This method is used to test that disposing the sink uploads the current interval, and that a restart within the
    /// same interval continues the local file, so the interval still ends up in one object.
    /// </summary>
    /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
    [TestMethod]
    public async Task DisposingUploadsTheCurrentIntervalAndARestartContinuesIt()
    {
        var directory = CreateTestDirectory();

        try
        {
            using var client = new FakeAmazonS3Client();
            var clock = new FakeTimeProvider();

            using (var sink = CreateSink(client, directory, clock))
            {
                await sink.EmitBatchAsync([CreateLogEvent("Before restart")]);
            }

            Assert.AreEqual("log2026112410.txt", client.Keys.Single());
            Assert.AreEqual(1, Directory.GetFiles(directory).Length, "The file of the current interval has to be kept.");

            clock.UtcNow = Start.AddMinutes(30);

            using (var sink = CreateSink(client, directory, clock))
            {
                await sink.EmitBatchAsync([CreateLogEvent("After restart")]);
                clock.UtcNow = Start.AddHours(1);
                await sink.OnEmptyBatchAsync();
            }

            CollectionAssert.AreEqual(new List<string> { "log2026112410.txt", "log2026112410.txt" }, client.Keys);
            Assert.AreEqual($"Before restart{Environment.NewLine}After restart{Environment.NewLine}", client.Objects["log2026112410.txt"]);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    /// <summary>
    /// This method is used to test that a restart within the same interval but without the local file, a new container
    /// for example, does not overwrite the object uploaded before but takes the next sequence number.
    /// </summary>
    /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
    [TestMethod]
    public async Task ARestartWithoutTheLocalFileDoesNotOverwriteTheUploadedObject()
    {
        var directory = CreateTestDirectory();

        try
        {
            using var client = new FakeAmazonS3Client();
            var clock = new FakeTimeProvider();

            using (var sink = CreateSink(client, directory, clock))
            {
                await sink.EmitBatchAsync([CreateLogEvent("First run")]);
            }

            foreach (var file in Directory.GetFiles(directory))
            {
                File.Delete(file);
            }

            clock.UtcNow = Start.AddMinutes(30);

            using (var sink = CreateSink(client, directory, clock))
            {
                await sink.EmitBatchAsync([CreateLogEvent("Second run")]);
            }

            CollectionAssert.AreEqual(new List<string> { "log2026112410.txt", "log2026112410_001.txt" }, client.Keys);
            Assert.AreEqual($"First run{Environment.NewLine}", client.Objects["log2026112410.txt"]);
            Assert.AreEqual($"Second run{Environment.NewLine}", client.Objects["log2026112410_001.txt"]);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    /// <summary>
    /// This method is used to test that a local file of an interval that was over before the start, left behind by a
    /// process that did not shut down properly, is uploaded and removed.
    /// </summary>
    /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
    [TestMethod]
    public async Task AFileLeftFromAnEarlierIntervalIsUploaded()
    {
        var directory = CreateTestDirectory();

        try
        {
            File.WriteAllText(Path.Combine(directory, "log2026112409.txt"), "Left behind");

            using var client = new FakeAmazonS3Client();
            using var sink = CreateSink(client, directory, new FakeTimeProvider());

            await sink.OnEmptyBatchAsync();

            Assert.AreEqual("log2026112409.txt", client.Keys.Single());
            Assert.AreEqual("Left behind", client.Content);
            Assert.AreEqual(0, Directory.GetFiles(directory).Length, "The uploaded file was not deleted.");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    /// <summary>
    /// This method is used to test that a file whose upload failed stays on the disk and is uploaded at the next try.
    /// </summary>
    /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
    [TestMethod]
    public async Task AFailedUploadIsKeptAndTriedAgain()
    {
        var directory = CreateTestDirectory();

        try
        {
            using var client = new FakeAmazonS3Client();
            var clock = new FakeTimeProvider();
            using var sink = CreateSink(client, directory, clock);

            await sink.EmitBatchAsync([CreateLogEvent("Kept")]);
            client.FailNextUpload = true;
            clock.UtcNow = Start.AddHours(1);
            await sink.OnEmptyBatchAsync();

            Assert.AreEqual(0, client.Keys.Count);
            Assert.AreEqual(1, Directory.GetFiles(directory).Length, "The file has to be kept after a failed upload.");

            clock.UtcNow = Start.AddHours(1).AddSeconds(2);
            await sink.OnEmptyBatchAsync();

            Assert.AreEqual("log2026112410.txt", client.Keys.Single());
            Assert.AreEqual($"Kept{Environment.NewLine}", client.Content);
            Assert.AreEqual(0, Directory.GetFiles(directory).Length, "The uploaded file was not deleted.");
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

            using (var sink = CreateSink(client, directory, new FakeTimeProvider(), bucketPath: "awsSubPath"))
            {
                await sink.EmitBatchAsync([CreateLogEvent("Hello Amazon S3")]);
            }

            Assert.AreEqual("awsSubPath/log2026112410.txt", client.Keys.Single());
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    /// <summary>
    /// This method is used to test that an empty file is removed instead of being uploaded.
    /// </summary>
    /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
    [TestMethod]
    public async Task AnEmptyFileIsNotUploaded()
    {
        var directory = CreateTestDirectory();

        try
        {
            using var client = new FakeAmazonS3Client();
            var clock = new FakeTimeProvider();
            using var sink = CreateSink(client, directory, clock);

            await sink.EmitBatchAsync([]);
            clock.UtcNow = Start.AddHours(1);
            await sink.OnEmptyBatchAsync();

            Assert.AreEqual(0, client.Keys.Count);
            Assert.AreEqual(0, Directory.GetFiles(directory).Length, "The empty file was not deleted.");
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

            using (var sink = CreateSink(client, directory, new FakeTimeProvider()))
            {
                await sink.EmitBatchAsync([CreateLogEvent("Hello Amazon S3")]);
            }

            StringAssert.StartsWith(client.Content, "Hello Amazon S3");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    /// <summary>
    /// This method is used to test that <see cref="RollingInterval.Infinite"/>, whose interval is never over, keeps one
    /// file and uploads it when the sink is disposed.
    /// </summary>
    /// <returns>A <see cref="Task"/> returning any asynchronous operation.</returns>
    [TestMethod]
    public async Task WithAnInfiniteIntervalTheFileIsUploadedWhenTheSinkIsDisposed()
    {
        var directory = CreateTestDirectory();

        try
        {
            using var client = new FakeAmazonS3Client();
            var clock = new FakeTimeProvider();

            using (var sink = CreateSink(client, directory, clock, rollingInterval: RollingInterval.Infinite))
            {
                await sink.EmitBatchAsync([CreateLogEvent("First")]);
                clock.UtcNow = Start.AddDays(1);
                await sink.EmitBatchAsync([CreateLogEvent("Second")]);
                await sink.OnEmptyBatchAsync();

                Assert.AreEqual(0, client.Keys.Count, "Nothing may be uploaded before the sink is disposed.");
            }

            Assert.AreEqual("log.txt", client.Keys.Single());
            Assert.AreEqual($"First{Environment.NewLine}Second{Environment.NewLine}", client.Content);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    /// <summary>
    /// This method is used to test that the formatter passed to <c>WriteTo.AmazonS3</c> formats the uploaded file.
    /// See https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/issues/61.
    /// </summary>
    [TestMethod]
    public void TheConfiguredFormatterFormatsTheUploadedFile()
    {
        var directory = CreateTestDirectory();

        try
        {
            using var client = new FakeAmazonS3Client();

            // Disposing the logger flushes the batching sink and disposes the sink, so the upload has happened afterwards.
            using (var logger = new LoggerConfiguration().WriteTo.AmazonS3(
                client,
                Path.Combine(directory, "log.json"),
                "mytestbucket-aws",
                formatter: new CompactJsonFormatter())
                .CreateLogger())
            {
                logger.Information("Hello {Target}", "Amazon S3");
            }

            StringAssert.StartsWith(client.Content, "{\"@t\":");
            StringAssert.Contains(client.Content, "\"@mt\":\"Hello {Target}\"");
            StringAssert.Contains(client.Content, "\"Target\":\"Amazon S3\"");
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
    /// Creates a sink that writes into the given directory, uploads with the given client and takes the time from the
    /// given clock.
    /// </summary>
    /// <param name="client">The Amazon S3 client to use.</param>
    /// <param name="directory">The directory the local files are written to.</param>
    /// <param name="clock">The clock the rolling intervals are taken from.</param>
    /// <param name="bucketPath">The bucket path or <c>null</c> for none.</param>
    /// <param name="rollingInterval">The rolling interval.</param>
    /// <returns>The <see cref="AmazonS3Sink"/>.</returns>
    private static AmazonS3Sink CreateSink(
        AmazonS3Client client,
        string directory,
        TimeProvider clock,
        string? bucketPath = null,
        RollingInterval rollingInterval = RollingInterval.Hour)
    {
        var options = new AmazonS3Options
        {
            AmazonS3Client = client,
            BucketName = "mytestbucket-aws",
            BucketPath = bucketPath,
            Path = Path.Combine(directory, "log.txt"),
            RollingInterval = rollingInterval,
            OutputTemplate = "{Message:lj}{NewLine}"
        };

        return new AmazonS3Sink(options, clock);
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
