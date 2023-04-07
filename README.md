Serilog.Sinks.AmazonS3
====================================

Serilog.Sinks.AmazonS3 is a library to save logging information from [Serilog](https://github.com/serilog/serilog) to [Amazon S3](https://aws.amazon.com/s3/).
The idea there was to upload log files to [Amazon S3](https://aws.amazon.com/s3/) to later evaluate them with [Amazon EMR](https://aws.amazon.com/emr/) services.
This project makes use of the [Serilog.Sinks.File](https://github.com/serilog/serilog-sinks-file)'s code in a major part,
so thanks to all the [contributors](https://github.com/serilog/serilog-sinks-file/graphs/contributors) of this project :thumbsup:.

[![GitHub issues](https://img.shields.io/github/issues/serilog-contrib/Serilog.Sinks.AmazonS3.svg)](https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/issues)
[![GitHub forks](https://img.shields.io/github/forks/serilog-contrib/Serilog.Sinks.AmazonS3.svg)](https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/network)
[![GitHub stars](https://img.shields.io/github/stars/serilog-contrib/Serilog.Sinks.AmazonS3.svg)](https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/stargazers)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://raw.githubusercontent.com/serilog-contrib/Serilog.Sinks.AmazonS3/master/License.txt)
[![Nuget](https://img.shields.io/badge/Serilog.Sinks.AmazonS3-Nuget-brightgreen.svg)](https://www.nuget.org/packages/Serilog.Sinks.AmazonS3/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Serilog.Sinks.AmazonS3.svg)](https://www.nuget.org/packages/Serilog.Sinks.AmazonS3/)
[![Known Vulnerabilities](https://snyk.io/test/github/serilog-contrib/Serilog.Sinks.AmazonS3/badge.svg)](https://snyk.io/test/github/serilog-contrib/Serilog.Sinks.AmazonS3)
[![Gitter](https://badges.gitter.im/Serilog-Sinks-AmazonS3/community.svg)](https://gitter.im/Serilog-Sinks-AmazonS3/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)
<!-- ALL-CONTRIBUTORS-BADGE:START - Do not remove or modify this section -->
[![All Contributors](https://img.shields.io/badge/all_contributors-13-orange.svg?style=flat-square)](#contributors-)
<!-- ALL-CONTRIBUTORS-BADGE:END -->

## Available for
* NetStandard 2.0
* NetStandard 2.1
* Net 6.0
* Net 7.0

## Net Core and Net Framework latest and LTS versions
* https://dotnet.microsoft.com/download/dotnet-core
* https://dotnet.microsoft.com/download/dotnet

## Basic usage
Check out the how to use file [here](https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/blob/master/HowToUse.md).

## Install

```bash
dotnet add package Serilog.Sinks.AmazonS3
```

## Special notice for read-only file systems (e.g. AWS Lambda)
AWS Lambda exposes the `tmp` folder for writing, so prepending `/tmp/` to the path parameter of the AmazonS3 extension method allowed this sink to work with AWS Lambda.
Otherwise a similar exception is thrown:
```log
Exception while emitting periodic batch from Serilog.Sinks.PeriodicBatching.PeriodicBatchingSink: System.IO.IOException: Read-only file system
```

Change history
--------------

See the [Changelog](https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/blob/master/Changelog.md).

## Contributors ✨

Thanks goes to these wonderful people ([emoji key](https://allcontributors.org/docs/en/emoji-key)):

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tr>
    <td align="center"><a href="https://github.com/Galouw"><img src="https://avatars.githubusercontent.com/u/6368030?v=4?s=100" width="100px;" alt=""/><br /><sub><b>Galouw</b></sub></a><br /><a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=Galouw" title="Code">💻</a> <a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=Galouw" title="Documentation">📖</a></td>
    <td align="center"><a href="https://github.com/profet23"><img src="https://avatars.githubusercontent.com/u/2411974?v=4?s=100" width="100px;" alt=""/><br /><sub><b>Ben Grabkowitz</b></sub></a><br /><a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=profet23" title="Code">💻</a> <a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=profet23" title="Documentation">📖</a></td>
    <td align="center"><a href="https://github.com/aherrmann13"><img src="https://avatars.githubusercontent.com/u/1924089?v=4?s=100" width="100px;" alt=""/><br /><sub><b>Adam Herrmann</b></sub></a><br /><a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=aherrmann13" title="Code">💻</a> <a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=aherrmann13" title="Documentation">📖</a></td>
    <td align="center"><a href="https://github.com/qrczak0"><img src="https://avatars.githubusercontent.com/u/45206900?v=4?s=100" width="100px;" alt=""/><br /><sub><b>qrczak0</b></sub></a><br /><a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=qrczak0" title="Code">💻</a> <a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=qrczak0" title="Documentation">📖</a></td>
    <td align="center"><a href="http://longfin.github.com"><img src="https://avatars.githubusercontent.com/u/128436?v=4?s=100" width="100px;" alt=""/><br /><sub><b>Swen Mun</b></sub></a><br /><a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=longfin" title="Code">💻</a> <a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=longfin" title="Documentation">📖</a></td>
    <td align="center"><a href="https://www.linkedin.com/in/barrymooring/"><img src="https://avatars.githubusercontent.com/u/1089628?v=4?s=100" width="100px;" alt=""/><br /><sub><b>Barry Mooring</b></sub></a><br /><a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=codingbadger" title="Code">💻</a> <a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=codingbadger" title="Documentation">📖</a></td>
    <td align="center"><a href="https://github.com/samburville"><img src="https://avatars.githubusercontent.com/u/7041731?v=4?s=100" width="100px;" alt=""/><br /><sub><b>Sam Burville</b></sub></a><br /><a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=samburville" title="Code">💻</a> <a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=samburville" title="Documentation">📖</a></td>
  </tr>
  <tr>
    <td align="center"><a href="https://github.com/stylesm"><img src="https://avatars.githubusercontent.com/u/5602910?v=4?s=100" width="100px;" alt=""/><br /><sub><b>Matt Styles</b></sub></a><br /><a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=stylesm" title="Code">💻</a> <a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=stylesm" title="Documentation">📖</a></td>
    <td align="center"><a href="http://vlecerf.com"><img src="https://avatars.githubusercontent.com/u/7376668?v=4?s=100" width="100px;" alt=""/><br /><sub><b>Valentin LECERF</b></sub></a><br /><a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=ioxFR" title="Code">💻</a> <a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=ioxFR" title="Documentation">📖</a></td>
    <td align="center"><a href="https://franzhuber23.blogspot.de/"><img src="https://avatars.githubusercontent.com/u/9639361?v=4?s=100" width="100px;" alt=""/><br /><sub><b>HansM</b></sub></a><br /><a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=SeppPenner" title="Code">💻</a> <a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=SeppPenner" title="Documentation">📖</a> <a href="#example-SeppPenner" title="Examples">💡</a> <a href="#maintenance-SeppPenner" title="Maintenance">🚧</a> <a href="#projectManagement-SeppPenner" title="Project Management">📆</a> <a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=SeppPenner" title="Tests">⚠️</a></td>
    <td align="center"><a href="https://github.com/serilog-contrib"><img src="https://avatars.githubusercontent.com/u/78050538?v=4?s=100" width="100px;" alt=""/><br /><sub><b>Serilog Contrib</b></sub></a><br /><a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=serilog-contrib" title="Code">💻</a> <a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=serilog-contrib" title="Documentation">📖</a> <a href="#example-serilog-contrib" title="Examples">💡</a> <a href="#maintenance-serilog-contrib" title="Maintenance">🚧</a> <a href="#projectManagement-serilog-contrib" title="Project Management">📆</a> <a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=serilog-contrib" title="Tests">⚠️</a></td>
    <td align="center"><a href="https://github.com/seruminar"><img src="https://avatars.githubusercontent.com/u/35008875?v=4?s=100" width="100px;" alt=""/><br /><sub><b>Yuriy Sountsov</b></sub></a><br /><a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=seruminar" title="Documentation">📖</a></td>
    <td align="center"><a href="https://github.com/kosovrzn"><img src="https://avatars.githubusercontent.com/u/13337834?v=4?s=100" width="100px;" alt=""/><br /><sub><b>Sergey Kosov</b></sub></a><br /><a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=kosovrzn" title="Code">💻</a> <a href="https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/commits?author=kosovrzn" title="Tests">⚠️</a></td>
  </tr>
</table>

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->

This project follows the [all-contributors](https://github.com/all-contributors/all-contributors) specification. Contributions of any kind welcome!
