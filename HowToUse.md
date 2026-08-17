## Basic usage
```csharp
var logger = new LoggerConfiguration().WriteTo
    .AmazonS3(
        "log.txt",
        "mytestbucket-aws",
        Amazon.RegionEndpoint.EUWest2,
        "ABCDEFGHIJKLMNOP",
        "c3fghsrgwegfn://asdfsdfsdgfsdg",
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Minute)
    .CreateLogger();

for (var x = 0; x < 200; x++)
{
    var ex = new Exception("Test");
    logger.Error(ex.ToString());
}
```

Either `outputTemplate` or `formatter` has to be part of the call, and by name. Both exist in their
own set of overloads, and without one of them the compiler cannot tell which overload is meant and
reports `CS0121`. Passing `outputTemplate: null` is allowed as well, the sink then uses the default
template shown above.

## Usage with role based authentication in AWS
Use this method if you gave access to Amazon S3 from your AWS program execution machine using roles. In this case, authorization is managed by AWS and `awsAccessKeyId` and `awsSecretAccessKey` are not required.

```csharp
var logger = new LoggerConfiguration().WriteTo
    .AmazonS3(
        "log.txt",
        "mytestbucket-aws",
        Amazon.RegionEndpoint.EUWest2,
        outputTemplate: null,
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
var logger = new LoggerConfiguration()
    .WriteTo.AmazonS3(
        "log.json",
        "mytestbucket-aws",
        Amazon.RegionEndpoint.EUWest2,
        formatter: new CompactJsonFormatter(),
        rollingInterval: RollingInterval.Minute)
    .CreateLogger();

for (var x = 0; x < 200; x++)
{
    var ex = new Exception("Test");
    logger.Error(ex.ToString());
}
```

`formatter` picks the overloads that format the events themselves, `outputTemplate` and
`formatProvider` are not used then.

## Configuring from appsettings.json files
```json
"Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    },
    "WriteTo": [
      {
        "Name": "AmazonS3",
        "Args": {
          "path": "log.txt",
          "bucketName": "mybucket-aws",
          "rollingInterval": "Day",
          "serviceUrl": "https://s3.eu-west-2.amazonaws.com",
          "disablePayloadSigning": "false"
        }
      }
    ]
  }
```

For more information regarding this use case, see [Issue number 10](https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/issues/10) and [Serilog formatting JSON](https://github.com/serilog/serilog/wiki/Formatting-Output#formatting-json).


## Exception handling
The sink does not report failures to your application. Everything that goes wrong while a batch is
uploaded, a wrong bucket name, missing rights or an unreachable endpoint, is caught by the periodic
batching sink and written to Serilog's `SelfLog`. Enable it to see those messages:

```csharp
Serilog.Debugging.SelfLog.Enable(Console.Error);
```

The `failureCallback` parameter that earlier versions offered is gone since version 1.6.0.0. Use
fallback logging instead, so that events survive when this sink cannot deliver them. Check
https://nblumhardt.com/2024/10/fallback-logging/.

The project can be found on [nuget](https://www.nuget.org/packages/Serilog.Sinks.AmazonS3/).

## Configuration options

|Parameter|Meaning|Example|Default value|
|-|-|-|-|
|client|The Amazon S3 client. It will be created in the sink from the given options if the specified one is `null`.|`new AmazonS3Client()`|`null`|
|formatter|The formatter that can be implemented as desired. See [Issue number 10](https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/issues/10) and [Serilog formatting JSON](https://github.com/serilog/serilog/wiki/Formatting-Output#formatting-json) for more information.|`new CompactJsonFormatter()`|None, is optional.|
|path|The main log file name used.|`"log.txt"`|None, is mandatory.|
|bucketName|The name of the Amazon S3 bucket to use.<br>Check: https://docs.aws.amazon.com/general/latest/gr/rande.html.|`"mytestbucket-aws"`|None, is mandatory.|
|endpoint|The Amazon S3 endpoint location.|`RegionEndpoint.EUWest2`|None, is mandatory. (Either `endpoint` or `serviceUrl` needs to be set.)|
|serviceUrl|The Amazon S3 service URL.<br>Check: https://docs.aws.amazon.com/general/latest/gr/s3.html.|`https://s3.amazonaws.com`|Default is `https://s3.amazonaws.com`. (Either `endpoint` or `serviceUrl` needs to be set.)|
|awsAccessKeyId|The Amazon S3 access key id.|`ABCDEFGHIJKLMNOP`|None, is mandatory. (Not required if you are using role based authentication).|
|awsSecretAccessKey|The Amazon S3 secret access key.|`c3fghsrgwegfn://asdfsdfsdgfsdg`|None, is mandatory. (Not required if you are using role based authentication).|
|restrictedToMinimumLevel|The minimum level for events passed through the sink. Ignored when `levelSwitch` is specified.<br>Check: https://github.com/serilog/serilog/blob/dev/src/Serilog/Events/LogEventLevel.cs.|`LogEventLevel.Information`|`LogEventLevel.Verbose`|
|outputTemplate|A message template describing the format used to write to the sink.|`"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"`|`"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"`. If `formatter` is specified: Not needed.|
|formatProvider|The `IFormatProvider` to use. Supplies culture-specific formatting information.<br>Check: https://docs.microsoft.com/en-us/dotnet/api/system.iformatprovider?view=netframework-4.8.|`new CultureInfo("de-DE")`|`null`.  If `formatter` is specified: Not needed.|
|levelSwitch|A switch allowing the pass-through minimum level to be changed at runtime.<br>Check: https://nblumhardt.com/2014/10/dynamically-changing-the-serilog-level/.|`var levelSwitch = new LoggingLevelSwitch(); levelSwitch.MinimumLevel = LogEventLevel.Warning;`|`null`|
|rollingInterval|The interval at which logging will roll over to a new file.<br>Check: https://github.com/serilog/serilog-sinks-file/blob/dev/src/Serilog.Sinks.File/RollingInterval.cs.|`rollingInterval: RollingInterval.Minute`|`RollingInterval.Day`|
|encoding|Character encoding used to write the text file.<br>Check: https://docs.microsoft.com/de-de/dotnet/api/system.text.encoding?view=netframework-4.8.|`encoding: Encoding.Unicode`|`null` meaning UTF-8 without a byte order mark|
|~~failureCallback~~|~~Adds an option to add a failure callback action.~~  (Removed in version 1.6.0.0, use fallback logging instead. Check https://nblumhardt.com/2024/10/fallback-logging/.)|~~`failureCallback: e => Console.WriteLine($"Sink error: {e.Message}")`~~|~~`null`~~|
|bucketPath|Optionally add a sub-path for the bucket. Files are stored on S3 `mytestbucket-aws/awsSubPath/log.txt` in the example below.|`bucketPath: "awsSubPath"`|`null`|
|batchSizeLimit|The maximum number of events to include in a single batch. This means an upload of events as a file to S3 will contain at most this number of events.<br>Check: https://github.com/serilog/serilog-sinks-periodicbatching|`batchSizeLimit: 20`|`100`|
|batchingPeriod|The time to wait between checking for unemitted events. If there are any unemitted events, they will then be uploaded to S3 in a batch of maximum size `batchSizeLimit`.<br>Check: https://github.com/serilog/serilog-sinks-periodicbatching|`batchingPeriod: TimeSpan.FromSeconds(5)`|`TimeSpan.FromSeconds(2)`|
|eagerlyEmitFirstEvent|A value indicating whether the first event should be emitted immediately or not.|`eagerlyEmitFirstEvent: false`|`true`|
|queueSizeLimit|The queue size limit meaning the limit until the last not emitted events are discarded (Standard mechanims to stop queue overflows).|`queueSizeLimit: 2000`|`10000`|
|disablePayloadSigning|Setting `disablePayloadSigning` to `true` disables the Amazon S3 SigV4 payload signing data integrity check on each upload request. This option is provided if you are using other cloud storage providers e.g. Cloudflare R2 and they support AWS S3 APIs but currently lack support for the Streaming SigV4 implementation used by AWSSDK.S3. Since version 1.7.0.0 it also sets `RequestChecksumCalculation` to `WHEN_REQUIRED` on the client the sink builds, because AWSSDK.S3 4.x adds a checksum to every request by default and those providers usually reject it. If you pass your own `client`, set that on its config yourself.|`disablePayloadSigning: true`| Default is `false`. Even a null value will also result in a `false` setting.|

Hint: Only `outputTemplate` and `formatProvider` together or the `formatter` can be used.

## Bigger example

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
        levelSwitch: levelSwitch,
        rollingInterval: RollingInterval.Minute,
        encoding: Encoding.Unicode,
        bucketPath: "awsSubPath")
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
