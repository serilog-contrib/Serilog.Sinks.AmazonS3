Serilog.Sinks.AmazonS3
====================================

Serilog.Sinks.AmazonS3 is a library to save logging information from [Serilog](https://github.com/serilog/serilog) to [Amazon S3](https://aws.amazon.com/s3/).
The idea there was to upload log files to [Amazon S3](https://aws.amazon.com/s3/) to later evaluate them with [Amazon EMR](https://aws.amazon.com/emr/) services.
The assembly was written and tested in .Net Framework 4.8 and .Net Standard 2.0.
This project makes use of the [Serilog.Sinks.File](https://github.com/serilog/serilog-sinks-file)'s code in a major part,
so thanks to all the [contributors](https://github.com/serilog/serilog-sinks-file/graphs/contributors) of this project :thumbsup:.

[![Build status](https://ci.appveyor.com/api/projects/status/kefc5a2lyvet88yx?svg=true)](https://ci.appveyor.com/project/SeppPenner/serilog-sinks-amazons3)
[![GitHub issues](https://img.shields.io/github/issues/SeppPenner/Serilog.Sinks.AmazonS3.svg)](https://github.com/SeppPenner/Serilog.Sinks.AmazonS3/issues)
[![GitHub forks](https://img.shields.io/github/forks/SeppPenner/Serilog.Sinks.AmazonS3.svg)](https://github.com/SeppPenner/Serilog.Sinks.AmazonS3/network)
[![GitHub stars](https://img.shields.io/github/stars/SeppPenner/Serilog.Sinks.AmazonS3.svg)](https://github.com/SeppPenner/Serilog.Sinks.AmazonS3/stargazers)
[![GitHub license](https://img.shields.io/badge/license-AGPL-blue.svg)](https://raw.githubusercontent.com/SeppPenner/Serilog.Sinks.AmazonS3/master/License.txt)
[![Nuget](https://img.shields.io/badge/Serilog.Sinks.AmazonS3-Nuget-brightgreen.svg)](https://www.nuget.org/packages/HaemmerElectronics.SeppPenner.Serilog.Sinks.AmazonS3/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/HaemmerElectronics.SeppPenner.Serilog.Sinks.AmazonS3.svg)](https://www.nuget.org/packages/HaemmerElectronics.SeppPenner.Serilog.Sinks.AmazonS3/)
[![Known Vulnerabilities](https://snyk.io/test/github/SeppPenner/Serilog.Sinks.AmazonS3/badge.svg)](https://snyk.io/test/github/SeppPenner/Serilog.Sinks.AmazonS3)

## Available for
* NetFramework 4.5
* NetFramework 4.6
* NetFramework 4.6.2
* NetFramework 4.7
* NetFramework 4.7.2
* NetFramework 4.8
* NetStandard 2.0

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
Use this method if you gave access to S3 from your AWS program execution machine using roles. In this case, authorization is managed by AWS and the values `accessKey` and `accessSecret` are not required.

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

## Exception handling
You can pass a callback to the sink parameters on failure to define which action needs to be done if an exception occured on the sink side. If something is going wrong in the sink code like `access denied on a S3 bucket`, `failureCallback` will be executed.

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
|path|The main log file name used.|`"log.txt"`|None, is mandatory.|
|bucketName|The name of the Amazon S3 bucket to use. Check: https://docs.aws.amazon.com/general/latest/gr/rande.html.|`"mytestbucket-aws"`|None, is mandatory.|
|endpoint|The Amazon S3 endpoint location.|`RegionEndpoint.EUWest2`|None, is mandatory.|
|awsAccessKeyId|The Amazon S3 access key id.|`ABCDEFGHIJKLMNOP`|None.|
|awsSecretAccessKey|The Amazon S3 secret access key.|`c3fghsrgwegfn://asdfsdfsdgfsdg`|None.|
|restrictedToMinimumLevel|The minimum level for events passed through the sink. Ignored when `levelSwitch` is specified. Check: https://github.com/serilog/serilog/blob/dev/src/Serilog/Events/LogEventLevel.cs.|`LogEventLevel.Information`|`LogEventLevel.Verbose`|
|outputTemplate|A message template describing the format used to write to the sink.|`"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"`|`"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"`|
|formatProvider|The `IFormatProvider` to use. Supplies culture-specific formatting information. Check: https://docs.microsoft.com/en-us/dotnet/api/system.iformatprovider?view=netframework-4.8.|`new CultureInfo("de-DE")`|`null`|
|fileSizeLimitBytes|The number of bytes in a file. The sink rolls to a new file after the limit is reached.|`200`|`1L * 1024 * 1024 * 1024`|
|levelSwitch|A switch allowing the pass-through minimum level to be changed at runtime. Check: https://nblumhardt.com/2014/10/dynamically-changing-the-serilog-level/.|`var levelSwitch = new LoggingLevelSwitch(); levelSwitch.MinimumLevel = LogEventLevel.Warning;`|`null`|
|buffered|Indicates if flushing to the output file can be buffered or not.|`buffered: true`|`false`|
|rollingInterval|The interval at which logging will roll over to a new file. Check: https://github.com/serilog/serilog-sinks-file/blob/dev/src/Serilog.Sinks.File/RollingInterval.cs.|`rollingInterval: RollingInterval.Minute`|`RollingInterval.Day`|
|retainedFileCountLimit|The maximum number of log files that will be retained, including the current log file. For unlimited retention, pass `null`.|`10`|`31`|
|encoding|Character encoding used to write the text file. Check: https://docs.microsoft.com/de-de/dotnet/api/system.text.encoding?view=netframework-4.8.|`encoding: Encoding.Unicode`|`null` meaning `Encoding.UTF8`|
|hooks|Optionally enables hooking into log file lifecycle events. Check: https://github.com/serilog/serilog-sinks-file/blob/dev/src/Serilog.Sinks.File/Sinks/File/FileLifecycleHooks.cs and https://github.com/cocowalla/serilog-sinks-file-header/blob/master/src/Serilog.Sinks.File.Header/HeaderWriter.cs.|`hooks: new HeaderWriter("Timestamp,Level,Message")`|`null`|
|failureCallback| Optionally execute a callback if an exception has been throwed by the sink.|None.|

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
		failureCallback: e => Console.WriteLine($"An error occured in my sink: {e.Message}")
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

Change history
--------------

* **Version 1.0.2.0 (2019-07-19)** : Support of role based authorization to S3 added, failureCallback parameter added.
* **Version 1.0.1.0 (2019-06-23)** : Added icon to the nuget package.
* **Version 1.0.0.0 (2019-05-31)** : 1.0 release.
