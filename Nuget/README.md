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
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://raw.githubusercontent.com/SeppPenner/Serilog.Sinks.AmazonS3/master/License.txt)
[![Nuget](https://img.shields.io/badge/Serilog.Sinks.AmazonS3-Nuget-brightgreen.svg)](https://www.nuget.org/packages/HaemmerElectronics.SeppPenner.Serilog.Sinks.AmazonS3/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/HaemmerElectronics.SeppPenner.Serilog.Sinks.AmazonS3.svg)](https://www.nuget.org/packages/HaemmerElectronics.SeppPenner.Serilog.Sinks.AmazonS3/)
[![Known Vulnerabilities](https://snyk.io/test/github/SeppPenner/Serilog.Sinks.AmazonS3/badge.svg)](https://snyk.io/test/github/SeppPenner/Serilog.Sinks.AmazonS3)
[![Gitter](https://badges.gitter.im/Serilog-Sinks-AmazonS3/community.svg)](https://gitter.im/Serilog-Sinks-AmazonS3/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

## Available for
* NetFramework 4.6
* NetFramework 4.6.2
* NetFramework 4.7
* NetFramework 4.7.2
* NetFramework 4.8
* NetStandard 2.0
* NetStandard 2.1
* NetCore 2.1
* NetCore 3.1

## Net Core and Net Framework latest and LTS versions
* https://dotnet.microsoft.com/download/dotnet-framework
* https://dotnet.microsoft.com/download/dotnet-core

## Basic usage:
Check out the how to use file [here](https://github.com/SeppPenner/Serilog.Sinks.AmazonS3/blob/master/HowToUse.md).

Change history
--------------

* **Version 1.0.9.0 (2020-05-10)** : Updated nuget packages, added option to add standard and custom formatters.
* **Version 1.0.8.0 (2020-03-26)** : Updated nuget packages.
* **Version 1.0.7.0 (2020-02-09)** : Updated nuget packages, updated available versions.
* **Version 1.0.6.0 (2019-12-09)** : Fixed nuget package dependency bug.
* **Version 1.0.5.0 (2019-12-08)** : Updated nuget packages, added new option "bucketPath" to the sink.
* **Version 1.0.4.0 (2019-11-12)** : Small updates, added new option "autoUploadEvents" to the sink.
* **Version 1.0.3.0 (2019-11-08)** : Updated nuget packages, added GitVersionTask.
* **Version 1.0.2.0 (2019-07-19)** : Support of role based authorization to S3 added, failureCallback parameter added.
* **Version 1.0.1.0 (2019-06-23)** : Added icon to the nuget package.
* **Version 1.0.0.0 (2019-05-31)** : 1.0 release.
