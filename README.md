Serilog.Sinks.AmazonS3
====================================

Serilog.Sinks.AmazonS3 is a library to save logging information from [Serilog](https://github.com/serilog/serilog) to [Amazon S3](https://aws.amazon.com/s3/).
The idea there was to upload log files to [Amazon S3](https://aws.amazon.com/s3/) to later evaluate them with [Amazon EMR](https://aws.amazon.com/emr/) services.
The assembly was written and tested in .Net 5.0.
This project makes use of the [Serilog.Sinks.File](https://github.com/serilog/serilog-sinks-file)'s code in a major part,
so thanks to all the [contributors](https://github.com/serilog/serilog-sinks-file/graphs/contributors) of this project :thumbsup:.

[![Build status](https://ci.appveyor.com/api/projects/status/kefc5a2lyvet88yx?svg=true)](https://ci.appveyor.com/project/SeppPenner/serilog-sinks-amazons3)
[![GitHub issues](https://img.shields.io/github/issues/serilog-contrib/Serilog.Sinks.AmazonS3.svg)](https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/issues)
[![GitHub forks](https://img.shields.io/github/forks/serilog-contrib/Serilog.Sinks.AmazonS3.svg)](https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/network)
[![GitHub stars](https://img.shields.io/github/stars/serilog-contrib/Serilog.Sinks.AmazonS3.svg)](https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/stargazers)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://raw.githubusercontent.com/serilog-contrib/Serilog.Sinks.AmazonS3/master/License.txt)
[![Nuget](https://img.shields.io/badge/Serilog.Sinks.AmazonS3-Nuget-brightgreen.svg)](https://www.nuget.org/packages/Serilog.Sinks.AmazonS3/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Serilog.Sinks.AmazonS3.svg)](https://www.nuget.org/packages/Serilog.Sinks.AmazonS3/)
[![Known Vulnerabilities](https://snyk.io/test/github/serilog-contrib/Serilog.Sinks.AmazonS3/badge.svg)](https://snyk.io/test/github/serilog-contrib/Serilog.Sinks.AmazonS3)
[![Gitter](https://badges.gitter.im/Serilog-Sinks-AmazonS3/community.svg)](https://gitter.im/Serilog-Sinks-AmazonS3/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

## Available for
* NetFramework 4.6
* NetFramework 4.6.2
* NetFramework 4.7
* NetFramework 4.7.2
* NetFramework 4.8
* NetStandard 2.0
* NetStandard 2.1
* NetCore 3.1
* Net 5.0

## Net Core and Net Framework latest and LTS versions
* https://dotnet.microsoft.com/download/dotnet-framework
* https://dotnet.microsoft.com/download/dotnet-core
* https://dotnet.microsoft.com/download/dotnet

## Basic usage
Check out the how to use file [here](https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/blob/master/HowToUse.md).

## Install

```bash
dotnet add package Serilog.Sinks.AmazonS3
```

Change history
--------------

See the [Changelog](https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/blob/master/Changelog.md).
