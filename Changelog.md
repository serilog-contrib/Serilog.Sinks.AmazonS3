Change history
--------------

* **Version 1.6.0.0 (2025-03-24)** : Fixes https://github.com/serilog-contrib/Serilog.Sinks.Postgresql.Alternative/issues/83, updates NuGet packages, deprecates `failureCallback`.
* **Version 1.5.3.0 (2024-12-26)** : Removed support for Net6.0, added support for Net9.0, updated NuGet packages.
* **Version 1.5.2.0 (2024-05-16)** : Removed support for Net7.0.
* **Version 1.5.1.0 (2024-03-03)**: Updated NuGet packages.
* **Version 1.5.0.0 (2023-11-21)** : Updated NuGet packages, removed support for netstandard, added support for Net8.0.
* **Version 1.4.1.0 (2023-06-06)** : Updated NuGet packages, added option to disable Amazon S3 SigV4 payload signing (Thanks to [jawand](https://github.com/jawand)).
* **Version 1.4.0.0 (2023-04-07)** : Updated NuGet packages, removed support for NetCore3.1, hopefully fixes rolling interval bug from https://github.com/serilog-contrib/Serilog.Sinks.AmazonS3/issues/52.
* **Version 1.3.0.0 (2022-11-20)** : Updated NuGet packages, removed support for Net5.0, added support for Net7.0.
* **Version 1.2.9.0 (2022-10-30)** : Updated NuGet packages.
* **Version 1.2.8.0 (2022-08-28)** : Updated NuGet packages.
* **Version 1.2.7.0 (2022-08-03)** : Updated NuGet packages, fixed issue with service url (Thanks to @kosovrzn).
* **Version 1.2.6.0 (2022-06-01)** : Updated NuGet packages.
* **Version 1.2.5.0 (2022-04-04)** : Updated NuGet packages.
* **Version 1.2.4.0 (2022-02-16)** : Updated NuGet packages, added nullable checks, added editorconfig, added file scoped namespaces, added global usings, removed native support for Net Framework (Breaking change).
* **Version 1.2.3.0 (2022-01-12)** : NuGet packages updated.
* **Version 1.2.2.0 (2021-11-09)** : NuGet packages updated, added support for Net6.0.
* **Version 1.2.1.0 (2021-11-04)** : NuGet packages updated.
* **Version 1.2.0.0 (2021-09-12)** : Added example of loading logger config from appsettings (Thanks to @stylesm), added validation for the serviceUrl property (Thanks to @stylesm), updated icon, smaller adjustements due to move to the serilog-contrib organization, updated NuGet packages.
* **Version 1.1.11.0 (2021-09-03)** : Updated license to fit the new owning repository, updated readme and so on to fit new package name.
* **Version 1.1.10.0 (2021-08-29)** : Removed logging of S3 responses (Thanks to @KindOfANiceGuy), updated NuGet packages.
* **Version 1.1.9.0 (2021-08-09)** : Removed support for soon deprecated NetCore 2.1.
* **Version 1.1.8.0 (2021-07-25)** : Updated NuGet packages, enabled source linking for debugging.
* **Version 1.1.7.0 (2021-06-04)** : Updated NuGet packages.
* **Version 1.1.6.0 (2021-05-15)** : Updated NuGet packages.
* **Version 1.1.5.0 (2021-04-29)** : Updated NuGet packages.
* **Version 1.1.4.0 (2021-03-31)** : Updated NuGet packages, fixed a bug where the Service url wasn't injected correctly (Thanks to [aherrmann13](https://github.com/aherrmann13)).
* **Version 1.1.3.0 (2021-03-31)** : Updated NuGet packages, fixed a bug where the Amazon S3 client wasn't injected correctly (Thanks to [longfin](https://github.com/longfin)).
* **Version 1.1.2.0 (2021-02-27)** : Updated NuGet packages, added deletion of local files.
* **Version 1.1.1.0 (2021-02-21)** : Fixed a null reference exception with the formatter property.
* **Version 1.1.0.0 (2021-02-03)** : Big rework of the sink to make it work better. Check the options to configure the new sink.
* **Version 1.0.12.0 (2020-07-16)** : Added default AWS S3 serviceUrl.
* **Version 1.0.11.0 (2020-07-15)** : Updated NuGet packages, added new constructor taking serviceUrl instead of endpoint (Thanks to [Galouw](https://github.com/Galouw)).
* **Version 1.0.10.0 (2020-06-05)** : Updated NuGet packages, adjusted build to Visual Studio, moved changelog to extra file.
* **Version 1.0.9.0 (2020-05-10)** : Updated NuGet packages, added option to add standard and custom formatters.
* **Version 1.0.8.0 (2020-03-26)** : Updated NuGet packages.
* **Version 1.0.7.0 (2020-02-09)** : Updated NuGet packages, updated available versions.
* **Version 1.0.6.0 (2019-12-09)** : Fixed NuGet package dependency bug.
* **Version 1.0.5.0 (2019-12-08)** : Updated NuGet packages, added new option "bucketPath" to the sink.
* **Version 1.0.4.0 (2019-11-12)** : Small updates, added new option "autoUploadEvents" to the sink.
* **Version 1.0.3.0 (2019-11-08)** : Updated NuGet packages, added GitVersionTask.
* **Version 1.0.2.0 (2019-07-19)** : Support of role based authorization to S3 added, failureCallback parameter added.
* **Version 1.0.1.0 (2019-06-23)** : Added icon to the NuGet package.
* **Version 1.0.0.0 (2019-05-31)** : 1.0 release.