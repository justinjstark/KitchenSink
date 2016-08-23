#!/bin/bash

export EnableNuGetPackageRestore=true
mono .nuget/nuget.exe Install FAKE -OutputDirectory ./build/packages -ExcludeVersion
mono .nuget/nuget.exe Install GitVersion.CommandLine -OutputDirectory ./build/packages -ExcludeVersion
mono .nuget/nuget.exe Install Newtonsoft.Json -OutputDirectory ./build/packages -ExcludeVersion
mono build/packages/FAKE/tools/Fake.exe build.fsx "$@"
