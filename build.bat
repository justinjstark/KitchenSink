@echo off
cls
".nuget\NuGet.exe" "Install" "FAKE" "-OutputDirectory" "build\packages" "-ExcludeVersion"
"build\packages\FAKE\tools\Fake.exe" "build.fsx" "%1"
