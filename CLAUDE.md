# Project rules for Claude

## What this is

Serilog.Sinks.AmazonS3 is a Serilog sink that buffers log events, writes them into a local file and
uploads that file to an Amazon S3 bucket. The batching itself is not implemented here, it comes from
the NuGet package [Serilog.Sinks.PeriodicBatching](https://www.nuget.org/packages/Serilog.Sinks.PeriodicBatching/),
and the rolling file name logic is a modified copy of
[Serilog.Sinks.File](https://github.com/serilog/serilog-sinks-file). The repository **is** published
as the NuGet package `Serilog.Sinks.AmazonS3`: `GeneratePackageOnBuild` is on and
`BuildAndPushPackage.bat` pushes the result to nuget.org.

One solution `src/Serilog.Sinks.AmazonS3.sln` with exactly two projects:

- `src/Serilog.Sinks.AmazonS3/Serilog.Sinks.AmazonS3.csproj`, the library and the package.
- `src/Serilog.Sinks.AmazonS3.Tests/Serilog.Sinks.AmazonS3.Tests.csproj`, MSTest.

Layout inside `src/Serilog.Sinks.AmazonS3`:

- `LoggerConfigurationAmazonS3Extensions.cs`: the public entry point, ten `AmazonS3` overloads on
  `LoggerSinkConfiguration`. They differ in three dimensions: `RegionEndpoint` vs `serviceUrl` vs a
  ready made `AmazonS3Client`, with or without credentials, and `outputTemplate` plus
  `formatProvider` vs a ready made `ITextFormatter`. Every overload does its own argument checking,
  builds an `AmazonS3Options`, wraps the sink in a `PeriodicBatchingSink` and returns
  `sinkConfiguration.Sink(...)`. New parameters therefore have to be added ten times, that
  repetition is the price of the overload matrix.
- `Sinks/AmazonS3/AmazonS3Sink.cs`: the sink itself, an `IBatchedLogEventSink`.
  `EmitBatchAsync` opens the file, formats the batch into it, uploads it and deletes it.
  `OpenFile`, `AlignCurrentFileTo`, `GetFileName` and `UploadFileToS3` do one thing each, keep new
  logic in that shape.
- `Sinks/AmazonS3/AmazonS3Options.cs`: all options in one class, including the three properties
  `PathRoller`, `NextCheckpoint` and `CurrentFileSequence` that are internal state and documented as
  "not to be set by the options".
- `Sinks/AmazonS3/PathRoller.cs`, `RollingInterval.cs`, `RollingIntervalExtensions.cs`,
  `RollingLogFile.cs`: the rolling file name logic taken from Serilog.Sinks.File. These four files
  carry the Apache 2.0 header of the Serilog contributors on top of the MIT header, do not replace
  it.
- `Sinks/AmazonS3/FileInformation.cs`: the file name and writer pair that `OpenFile` returns.
- `Sinks/AmazonS3/ConfigurationValidator.cs` plus `ErrorMessageConstants.cs`: the service url check,
  both `internal`.
- `GlobalUsings.cs`: all usings of the project.

Layout inside `src/Serilog.Sinks.AmazonS3.Tests`:

- `AmazonS3BasicTests.cs`: five integration tests against a real bucket, driven by the environment
  variables `AwsAccessKeyId`, `AwsSecretAccessKey` and `AwsBucketName`. They report an inconclusive
  result when those are missing. Read "Known quirks" before you trust a green run.
- `AmazonS3SinkTests.cs`: the sink end to end without a network. `AmazonS3Client.PutObjectAsync` is
  virtual, so `FakeAmazonS3Client` overrides it and captures bucket name, key and bytes. These tests
  pin the file names of consecutive batches, the bucket path in front of the key, the deletion of
  the local file and the encoding.
- `PathRollerTests.cs` and `RollingIntervalExtensionsTests.cs`: the rolling file name logic.
- `LoggerConfigurationAmazonS3ExtensionsTests.cs`: what the ten overloads refuse. Every call there
  needs enough named arguments to pick one overload, `outputTemplate` or `formatter` usually does
  it, otherwise the call is ambiguous.
- `GlobalUsings.cs`: all usings of the test project.

Repository root: `README.md` (badges, target frameworks, install), `HowToUse.md` (the actual usage
documentation with all configuration options), `Changelog.md`, `Updating.md` (five lines on the
release process), `License.txt` (MIT), `PolicyExample.json` (a minimal S3 bucket policy),
`Icon.png`, `.all-contributorsrc`, `BuildAndPushPackage.bat`, `Delete-BIN-OBJ-Folders.bat` and
`.gitattributes`. `README.md`, `HowToUse.md`, `Changelog.md`, `License.txt`, `PolicyExample.json`
and `Icon.png` are packed into the NuGet package, so a change to them changes the package.

## Build

```powershell
dotnet build src/Serilog.Sinks.AmazonS3.sln
```

```powershell
dotnet test src/Serilog.Sinks.AmazonS3.sln
```

- The library multi targets `net8.0;net10.0`, the test project targets `net10.0` only. Both values
  live in the two `.csproj` files, there is no `TargetFrameworks` property anywhere else. Keep the
  test project on the newest framework the library targets.
- `src/Directory.Build.props` exists but contains nothing except `GenerateDocumentationFile`. All
  other build properties live directly in the two `.csproj` files and are duplicated there. That one
  property covers the test project as well, so a test class without XML documentation breaks the
  build with `CS1591`, which counts as an error here.
- `TreatWarningsAsErrors` is enabled in both projects, so every warning breaks the build, NuGet
  warnings (`NU****`) from restore included. A clean build reports zero warnings, keep it that way.
- `NU1803` (HTTP source usage during restore) is the one warning suppressed via `NoWarn`. Fix
  warnings instead of extending that list. The NuGet audit runs with its defaults, so a vulnerable
  package fails the build too.
- Versions come from GitVersion.MsBuild out of the git tags, for example `1.6.1-1` for the first
  commit after tag `1.6.0`. Never edit a version property or an assembly version by hand.
- Restore needs nuget.org. A private feed is configured globally on this machine
  (`http://192.168.201.22:5000/v3/index.json`) and is not always reachable, restore then fails with
  `NU1301` and `NU1900`. Then build with an explicit source:
  `dotnet build src/Serilog.Sinks.AmazonS3.sln --source https://api.nuget.org/v3/index.json`.
  `dotnet test` does not accept `--source`, restore first and then run it with `--no-restore`.
- Tests are MSTest, in the single test project `src/Serilog.Sinks.AmazonS3.Tests`, which follows the
  same package set as the sibling repositories: `Microsoft.NET.Test.Sdk`, `MSTest.TestAdapter`,
  `MSTest.TestFramework` and `GitVersion.MsBuild`. Never claim a test run happened without running
  it.
- The package is built by every `dotnet build` because of `GeneratePackageOnBuild`. The `.nupkg` and
  the `.snupkg` land in `src/Serilog.Sinks.AmazonS3/bin/<configuration>` and are what
  `BuildAndPushPackage.bat` pushes.

## Code conventions

Follow the surrounding code, it is consistent throughout every file:

- File header comment block with `<copyright file="..." company="SeppPenner and the Serilog
  contributors">` and a `<summary>`, then the file-scoped namespace. The four files taken from
  Serilog.Sinks.File additionally carry the Apache 2.0 notice inside that block.
- XML doc comments on every type and every member, private members included, no exceptions. Public
  parameters of the `AmazonS3` overloads are documented with `<param>` in the same wording in all
  ten overloads, keep them in sync.
- `Nullable`, `ImplicitUsings` and `LangVersion latest` are enabled.
- New `using` directives go into the `GlobalUsings.cs` of the respective project, inside the
  existing `#pragma warning disable IDE0065` block, never at the top of a file. The editorconfig
  requires usings inside the namespace (`csharp_using_directive_placement=inside_namespace:warning`),
  which global usings cannot satisfy, that is what the pragma is for. Do not add other pragmas
  except the `Serilog004` ones the test project already uses. The comment text in that block is
  German because Visual Studio generated it, leave it alone.
- Fields, properties, methods and events are always accessed with `this.` qualification
  (`dotnet_style_qualification_for_*` at severity `warning`).
- `src/.editorconfig` also enforces braces everywhere, no multiple blank lines, four spaces, CRLF,
  UTF-8, file scoped namespaces, `System` usings sorted first and `IDE0005` as warning. Analyzer
  warnings are fixed, not silenced.
- The `RootNamespace` of the library is `Serilog`, not the assembly name. The extension class lives
  in namespace `Serilog` so that `WriteTo.AmazonS3(...)` works without an extra using, everything
  else lives in `Serilog.Sinks.AmazonS3`.

## Known quirks

Do not silently "clean up" these, they are existing behaviour:

- **A green `AmazonS3BasicTests` still proves very little.** Those five tests contain no `Assert`,
  they configure a logger, write 200 events and dispose it. Since the sink swallows every error (see
  below), they stay green whether the upload worked or not. They report an inconclusive result when
  the environment variables are missing, which at least separates "not configured" from "ran". The
  real assertions live in the other four test classes, which need neither credentials nor network.
  Never present a green run as proof that an upload arrived in the bucket.
- **Every exception inside the sink is swallowed.** `PeriodicBatchingSink` catches whatever
  `EmitBatchAsync` throws and writes it to Serilog's `SelfLog`. A failing upload, missing
  credentials or a wrong bucket name are invisible unless `SelfLog.Enable(...)` is called, which is
  exactly what `ClassInitialize` in the test class does. That is also why the sink can throw freely
  instead of returning error codes.
- **A new file per batch, not per rolling interval.** `EmitBatchAsync` calls
  `AlignCurrentFileTo(DateTime.Now, true)`, always with `nextSequence: true`, so every batch gets the
  next sequence number and its own file, for example `log20260817_001.txt`, `log20260817_002.txt`.
  The rolling interval only controls the date part of the name. The `return string.Empty` branch in
  `AlignCurrentFileTo` is therefore unreachable from the sink, it is a leftover of the Serilog file
  sink logic.
- **The first batch has no sequence number, and `RollingInterval.Infinite` has none at all.**
  `AlignCurrentFileTo` returns the plain name as long as `NextCheckpoint` is not set yet, so the
  keys of the first batches are `log20260817.txt`, `log20260817_001.txt`, `log20260817_002.txt`.
  With `RollingInterval.Infinite` there is no checkpoint at all, so every batch is uploaded as
  `log.txt` and overwrites the object of the batch before it. `AmazonS3SinkTests` pins the first
  half of that, the second half is a trap, not a feature.
- **The local file is a temporary file.** It is written next to `path`, uploaded and deleted right
  after. The bucket key is `bucketPath` plus the file name with backslashes replaced by slashes, the
  local directory structure never reaches S3.
- **`path` is a name pattern, not a file that is kept.** Callers pass `log.txt` and get
  `log<date>_<sequence>.txt` on S3, `PathRoller` splits the name into prefix and extension and puts
  the period and the sequence number in between.
- **An empty file is refused.** `UploadFileToS3` throws `InvalidOperationException` when the file
  length is 0 instead of uploading it, because S3 would happily store an empty object.
- **`disablePayloadSigning` also turns off the SDK checksums.** Since AWS SDK version 4 a checksum
  is calculated for every request by default. The providers that need the payload signing switched
  off, Cloudflare R2 for example, usually reject those checksums as well, so `GetOrCreateClient`
  sets `RequestChecksumCalculation.WHEN_REQUIRED` on the config it builds. That only applies to a
  client the sink creates itself, a client passed in through the configuration keeps whatever its
  own config says.
- **`Serilog.Debugging` sits in the library's `GlobalUsings.cs` unused.** `SelfLog` is only used in
  the test project. Unused global usings are not reported by `IDE0005`.
- **Ten overloads, one implementation.** The bodies of the ten `AmazonS3` extension methods are
  copies of each other. That is deliberate, the overloads exist so that the sink can be configured
  from `appsettings.json`, where Serilog matches parameter names.
- **Every call needs `outputTemplate` or `formatter` by name.** The template overload and the
  formatter overload of a pair differ only in optional parameters, so a call that names neither is
  `CS0121`, ambiguous. `outputTemplate: null` is enough, the sink falls back to its default template.
  That is not theory, two samples in `HowToUse.md` were written without it and did not compile. When
  you touch a sample or write a test, compile it.
- **`.gitattributes` sets `* text=auto`** and every rule of the Visual Studio template below it is
  commented out. There is no binary file in this repository that needs its own rule, `Icon.png` is
  detected by git itself.
- **`src/Serilog.Sinks.AmazonS3.sln.DotSettings`** is tracked and holds nothing but a ReSharper user
  dictionary (`amazonaws`, `destructors`, `Flushable`, `H_00E4mmer`, `Sepp`, `Serilog`). Leave it
  alone.
- **No CI in the repository.** There is no `.github` folder and no pipeline file, the badges in the
  README point at services configured outside of this repository.

## Releasing

1. Make the change.
2. Add an entry at the top of `Changelog.md` in the existing format:
   `* **Version 1.7.0.0 (2026-08-17)** : Short description.`
3. Update `PackageReleaseNotes` in `src/Serilog.Sinks.AmazonS3/Serilog.Sinks.AmazonS3.csproj` to the
   same text. It is the only place in the build files that names a version, and it is duplicated
   from the changelog by hand.
4. Commit that.
5. Tag the commit with the plain version number, no `v` prefix (`1.6.0`, `1.5.3`, ...). The existing
   tags are lightweight tags, create new ones the same way.
6. Push the commits and the tag.
7. Only then build the package. GitVersion takes the version from the tag, a build before the tag
   produces a prerelease version like `1.7.0-1+Branch.master.Sha...` and that version would end up
   on nuget.org.
8. `BuildAndPushPackage.bat` deletes all `bin` and `obj` folders, builds in release and pushes the
   `.nupkg` and the `.snupkg` to nuget.org with `%NUGET_API_KEY%`. It ends with `PAUSE`, so it wants
   a console.

The version in the `Changelog.md` has four parts (`1.7.0.0`), the tag has three (`1.7.0`).

## Git

- **Never amend a commit.** No `git commit --amend`, not for a typo in the message, not to add a
  forgotten file, not even when the commit is still local. Write a follow-up commit instead. The
  release versions come from tags on exact commits, an amended commit leaves its tag pointing at a
  commit that no longer exists in the branch.

## Writing style

- Commit messages are written **in English only**: short, precise subject line, explanatory body
  when needed.
- Code comments and comments in project files such as `.csproj` are **always English**, regardless
  of the language used in the conversation.
- **No em dashes or en dashes** (`—`, `–`), neither in prose, commit messages, code comments nor
  documentation. Use a regular hyphen, comma, colon, parentheses or a separate sentence.
- German texts (documentation, chat replies) always use real umlauts and ß, never ASCII
  transliterations such as `ae`, `oe`, `ue` or `ss`. Identifiers, file names and configuration keys
  stay unchanged where umlauts are technically undesirable.
