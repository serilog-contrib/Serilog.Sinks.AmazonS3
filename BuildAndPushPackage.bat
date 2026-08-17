cd .\src

@ECHO off
cls

FOR /d /r . %%d in (bin,obj) DO (
	IF EXIST "%%d" (		 	 
		ECHO %%d | FIND /I "\node_modules\" > Nul && ( 
			ECHO.Skipping: %%d
		) || (
			ECHO.Deleting: %%d
			rd /s/q "%%d"
		)
	)
)

@REM Echo stays off from here on. With echo on, cmd prints each command line after expanding the
@REM variables in it, which would put %NUGET_API_KEY% on the console in plain text.
@ECHO off
@ECHO.Building solution...
@dotnet restore
@dotnet build -c Release
@cd .\Serilog.Sinks.AmazonS3\bin\Release
@ECHO.Build successful.
@dotnet nuget push *.nupkg -s "nuget.org" --skip-duplicate -k "%NUGET_API_KEY%"
@dotnet nuget push *.snupkg -s "nuget.org" --skip-duplicate -k "%NUGET_API_KEY%"
@ECHO.Upload success. Press any key to exit.
PAUSE