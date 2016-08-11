// include Fake lib
#r @"build/packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Testing.NUnit3
open Fake.NuGet.Install
open Fake.AssemblyInfoFile

//RestorePackages()

// Directories
let buildDir  = @"./build/"
let testDir   = @"./test/"
let deployDir = @"./deploy/"
let packagesDir = @"./build/packages"

// tools
let fxCopRoot = @"./Tools/FxCop/FxCopCmd.exe"

// version info
let version = "0.0.1"  // Use GitVersion to get this

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; testDir; deployDir]
)

Target "SetVersions" (fun _ ->
    CreateCSharpAssemblyInfo "./src/KitchenSink.Web/Properties/AssemblyInfo.cs"
        [//Attribute.Title "Kitchen Sink Continuous Delivery"
         //Attribute.Description "An example of continous delivery with all the bells and whistles"
         //Attribute.Guid "A539B42C-CB9F-4a23-8E57-AF4E7CEE5BAA"
         //Attribute.Product "Calculator"
         Attribute.Version version
         Attribute.FileVersion version]

    CreateCSharpAssemblyInfo "./src/KitchenSink.Web.Tests/Properties/AssemblyInfo.cs"
        [//Attribute.Title "Calculator library"
         //Attribute.Description "Sample project for FAKE - F# MAKE"
         //Attribute.Guid "EE5621DB-B86B-44eb-987F-9C94BCC98441"
         //Attribute.Product "Calculator"
         Attribute.Version version
         Attribute.FileVersion version]
)

Target "CompileApp" (fun _ ->
    !! @"src/**/*.csproj"
      |> MSBuildRelease buildDir "Build"
      |> Log "AppBuild-Output: "
)

Target "CompileTests" (fun _ ->
    !! @"src/**/*.Tests.csproj"
      |> MSBuildDebug testDir "Build"
      |> Log "TestBuild-Output: "
)

Target "RunTests" (fun _ ->
    !! (testDir + @"/NUnit.Test.*.dll")
      |> NUnit (fun p ->
                 {p with
                   DisableShadowCopy = true;
                   OutputFile = testDir + @"TestResults.xml"})
)

Target "FxCop" (fun _ ->
    !! (buildDir + @"/**/*.dll")
      ++ (buildDir + @"/**/*.exe")
        |> FxCop (fun p ->
            {p with
                ReportFileName = testDir + "FXCopResults.xml";
                ToolPath = fxCopRoot})
)

Target "Zip" (fun _ ->
    !! (buildDir + "/**/*.*")
        -- "*.zip"
        |> Zip buildDir (deployDir + "Calculator." + version + ".zip")
)

// Dependencies
"Clean"
  ==> "SetVersions"
  ==> "CompileApp"
  ==> "FxCop"
  ==> "CompileTests"
  ==> "RunTests"
  ==> "Zip"

// start build
RunTargetOrDefault "SetVersions"
