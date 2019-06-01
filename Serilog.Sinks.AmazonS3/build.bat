dotnet build Serilog.Sinks.AmazonS3.sln -c Release
xcopy /s .\Serilog.Sinks.AmazonS3\bin\Release ..\Nuget\Source\
xcopy /s .\Help ..\Nuget\Documentation\
pause