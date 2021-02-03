## Basic usage:
```csharp
var logger = new LoggerConfiguration().WriteTo
    .AmazonS3(
        "log.txt",
        "mytestbucket-aws",
        Amazon.RegionEndpoint.EUWest2,
        "ABCDEFGHIJKLMNOP",
        "c3fghsrgwegfn://asdfsdfsdgfsdg",
        fileSizeLimitBytes: 200,
        rollingInterval: RollingInterval.Minute)
    .CreateLogger();

for (var x = 0; x < 200; x++)
{
    var ex = new Exception("Test");
    logger.Error(ex.ToString());
}
```

## Usage with role based authentication in AWS
Use this method if you gave access to Amazon S3 from your AWS program execution machine using roles. In this case, authorization is managed by AWS and `awsAccessKeyId` and `awsSecretAccessKey` are not required.

```csharp
var logger = new LoggerConfiguration().WriteTo
    .AmazonS3(
        "log.txt",
        "mytestbucket-aws",
        Amazon.RegionEndpoint.EUWest2,
        fileSizeLimitBytes: 200,
        rollingInterval: RollingInterval.Minute)
    .CreateLogger();

for (var x = 0; x < 200; x++)
{
    var ex = new Exception("Test");
    logger.Error(ex.ToString());
}
```

## Using JSON or custom formatters
```csharp
var levelSwitch = new LoggingLevelSwitch { MinimumLevel = LogEventLevel.Information };

var logger = new LoggerConfiguration()
	.WriteTo.AmazonS3(
		new CompactJsonFormatter(),
        "log.json",
        "mytestbucket-aws",
        Amazon.RegionEndpoint.EUWest2,
        fileSizeLimitBytes: 200,
        rollingInterval: RollingInterval.Minute,
	)
	.CreateLogger();

for (var x = 0; x < 200; x++)
{
	var ex = new Exception("Test");
	logger.Error(ex.ToString());
}
```

For more information regarding this use case, see [Issue number 10](https://github.com/SeppPenner/Serilog.Sinks.AmazonS3/issues/10) and [Serilog formatting JSON](https://github.com/serilog/serilog/wiki/Formatting-Output#formatting-json).


## Exception handling
You can pass a callback to the sink parameters on failure to define which action needs to be done if an exception occured on the sink side. If something is going wrong in the sink code, `failureCallback` will be executed.

```csharp
var logger = new LoggerConfiguration().WriteTo
    .AmazonS3(
        "log.txt",
        "mytestbucket-aws",
        Amazon.RegionEndpoint.EUWest2,
        fileSizeLimitBytes: 200,
        rollingInterval: RollingInterval.Minute,
		failureCallback: e => Console.WriteLine($"An error occured in my sink: {e.Message}")
		)
    .CreateLogger();

for (var x = 0; x < 200; x++)
{
    var ex = new Exception("Test");
    logger.Error(ex.ToString());
}
```

The project can be found on [nuget](https://www.nuget.org/packages/HaemmerElectronics.SeppPenner.Serilog.Sinks.AmazonS3/).

## Configuration options:

|Parameter|Meaning|Example|Default value|
|-|-|-|-|

|client|The Amazon S3 client. It will be created in the sink from the given options if the specified one is `null`.|`new AmazonS3Client()`|`null`|
|formatter|The formatter that can be implemented as desired. See [Issue number 10](https://github.com/SeppPenner/Serilog.Sinks.AmazonS3/issues/10) and [Serilog formatting JSON](https://github.com/serilog/serilog/wiki/Formatting-Output#formatting-json) for more information.|`new CompactJsonFormatter()`|None, is optional.|
|path|The main log file name used.|`"log.txt"`|None, is mandatory.|
|bucketName|The name of the Amazon S3 bucket to use. Check: https://docs.aws.amazon.com/general/latest/gr/rande.html.|`"mytestbucket-aws"`|None, is mandatory.|
|endpoint|The Amazon S3 endpoint location.|`RegionEndpoint.EUWest2`|None, is mandatory. (Either `endpoint` or `serviceUrl` needs to be set.)|
|serviceUrl|The Amazon S3 service URL.|`s3.amazonaws.com`|Default is `s3.amazonaws.com`. (Either `endpoint` or `serviceUrl` needs to be set.)|
|awsAccessKeyId|The Amazon S3 access key id.|`ABCDEFGHIJKLMNOP`|None, is mandatory. (Not required if you are using role based authentication).|
|awsSecretAccessKey|The Amazon S3 secret access key.|`c3fghsrgwegfn://asdfsdfsdgfsdg`|None, is mandatory. (Not required if you are using role based authentication).|
|restrictedToMinimumLevel|The minimum level for events passed through the sink. Ignored when `levelSwitch` is specified. Check: https://github.com/serilog/serilog/blob/dev/src/Serilog/Events/LogEventLevel.cs.|`LogEventLevel.Information`|`LogEventLevel.Verbose`|
|outputTemplate|A message template describing the format used to write to the sink.|`"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"`|`"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"`. If `formatter` is specified: Not needed.|
|formatProvider|The `IFormatProvider` to use. Supplies culture-specific formatting information. Check: https://docs.microsoft.com/en-us/dotnet/api/system.iformatprovider?view=netframework-4.8.|`new CultureInfo("de-DE")`|`null`.  If `formatter` is specified: Not needed.|
|fileSizeLimitBytes|The number of bytes in a file. The sink rolls to a new file after the limit is reached.|`200`|`1L * 1024 * 1024 * 1024`|
|levelSwitch|A switch allowing the pass-through minimum level to be changed at runtime. Check: https://nblumhardt.com/2014/10/dynamically-changing-the-serilog-level/.|`var levelSwitch = new LoggingLevelSwitch(); levelSwitch.MinimumLevel = LogEventLevel.Warning;`|`null`|
|buffered|Indicates if flushing to the output file can be buffered or not.|`buffered: true`|`false`|
|rollingInterval|The interval at which logging will roll over to a new file. Check: https://github.com/serilog/serilog-sinks-file/blob/dev/src/Serilog.Sinks.File/RollingInterval.cs.|`rollingInterval: RollingInterval.Minute`|`RollingInterval.Day`|
|retainedFileCountLimit|The maximum number of log files that will be retained, including the current log file. For unlimited retention, pass `null`.|`10`|`31`|
|encoding|Character encoding used to write the text file. Check: https://docs.microsoft.com/de-de/dotnet/api/system.text.encoding?view=netframework-4.8.|`encoding: Encoding.Unicode`|`null` meaning `Encoding.UTF8`|
|hooks|Optionally enables hooking into log file lifecycle events. Check: https://github.com/serilog/serilog-sinks-file/blob/dev/src/Serilog.Sinks.File/Sinks/File/FileLifecycleHooks.cs and https://github.com/cocowalla/serilog-sinks-file-header/blob/master/src/Serilog.Sinks.File.Header/HeaderWriter.cs.|`hooks: new HeaderWriter("Timestamp,Level,Message")`|`null`|
|autoUploadEvents|A flag indicating whether the log events file is updated immediately to Amazon S3 after a new log event is created.|`true`|`false`|
|failureCallback|Optionally execute a callback if an exception has been throwed by the sink.|`failureCallback: e => Console.WriteLine($"An error occured in my sink: {e.Message}"))`|None.|
|bucketPath|Optionally add a sub-path for the bucket. Files are stored on S3 `mytestbucket-aws/awsSubPath/log.txt` in the example below.|`bucketPath = "awsSubPath"`|`null`|

Hint: Only `outputTemplate` and `formatProvider` together or the `formatter` can be used.

## Full example

```csharp
var levelSwitch = new LoggingLevelSwitch();
levelSwitch.MinimumLevel = LogEventLevel.Warning;

var logger = new LoggerConfiguration().WriteTo
    .AmazonS3(
        "log.txt",
        "mytestbucket-aws",
        Amazon.RegionEndpoint.EUWest2,
        "ABCDEFGHIJKLMNOP",
        "c3fghsrgwegfn://asdfsdfsdgfsdg",
        restrictedToMinimumLevel:LogEventLevel.Verbose,
        outputTemplate:"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        new CultureInfo("de-DE"),
        fileSizeLimitBytes: 200,
        levelSwitch: levelSwitch,
        buffered: true,
        rollingInterval: RollingInterval.Minute,
        retainedFileCountLimit: 10,
        encoding: Encoding.Unicode,
        hooks: new HeaderWriter("Timestamp,Level,Message"),
		failureCallback: e => Console.WriteLine($"An error occured in my sink: {e.Message}"),
		bucketPath = "awsSubPath"
		)
    .CreateLogger();

for (var x = 0; x < 200; x++)
{
    var ex = new Exception("Test");
    logger.Error(ex.ToString());
}
```

## Further links

* Overview over the Amazon endpoints and locations: https://docs.aws.amazon.com/general/latest/gr/rande.html
* How to prepare your S3 bucket to access it with a software: https://www.c-sharpcorner.com/article/fileupload-to-aws-s3-using-asp-net/
* Example on how to use the Amazon S3 API for .Net: https://stackoverflow.com/questions/25814972/how-to-upload-a-file-to-amazon-s3-super-easy-using-c-sharp
* AWS authorizations for requests: https://docs.aws.amazon.com/AmazonS3/latest/dev/access-control-auth-workflow-object-operation.html