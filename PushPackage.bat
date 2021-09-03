dotnet nuget push "src\Serilog.Sinks.AmazonS3\bin\Release\Serilog.Sinks.AmazonS3.*.nupkg" -s "github" --skip-duplicate
dotnet nuget push "src\Serilog.Sinks.AmazonS3\bin\Release\Serilog.Sinks.AmazonS3.*.nupkg" -s "nuget.org" --skip-duplicate -k "%NUGET_API_KEY%"
PAUSE