#!/bin/bash

export EnableNuGetPackageRestore=true
mono .nuget/nuget.exe Install FAKE -OutputDirectory ./build/packages -ExcludeVersion
mono build/packages/FAKE/tools/Fake.exe build.fsx "$@"
