// include Fake lib
#r @"build/packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Testing.NUnit3
open Fake.NuGet.Install
open Fake.AssemblyInfoFile

//RestorePackages()

// Directories
let outputDir  = @"./build/output/"
let testDir   = @"./build/test/"
let deployDir = @"./build/deploy/"
let packagesDir = @"./build/packages"

// tools
let fxCopExe = @"./Tools/FxCop/FxCopCmd.exe"

let gitVersionExe = findToolInSubPath "GitVersion.exe" (packagesDir @@ "GitVersion.CommandLine")  

let release =
    ReadFile "RELEASE_NOTES.md"
    |> ReleaseNotesHelper.parseReleaseNotes

// version info
let releaseNotesVersion = release.AssemblyVersion
log ("ReleaseNotesVersion: " + releaseNotesVersion)

let gitVersion = GitVersionHelper.GitVersion (fun p ->
    { p with ToolPath = findToolInSubPath "GitVersion.exe" (packagesDir @@ "GitVersion.CommandLine") } //packagesDir @@ "/GitVersion.CommandLine/tools/GitVersion.exe"}
)
log ("GitVersion: " + gitVersion.FullSemVer)

if gitVersion.SemVer <> releaseNotesVersion then
    let e = FAKEException("GitVersion and ReleaseNotes versions do not match")
    traceException(e)
    raise(e)

// Targets
Target "Clean" (fun _ ->
    CleanDirs [outputDir; testDir; deployDir]
)

Target "SetVersions" (fun _ ->
    CreateCSharpAssemblyInfo "./src/KitchenSink.Web/Properties/AssemblyInfo.cs"
        [Attribute.Version gitVersion.AssemblySemVer
         Attribute.FileVersion gitVersion.AssemblySemVer]

    CreateCSharpAssemblyInfo "./src/KitchenSink.Web.Tests/Properties/AssemblyInfo.cs"
        [Attribute.Version gitVersion.AssemblySemVer
         Attribute.FileVersion gitVersion.AssemblySemVer]
)

Target "CompileApp" (fun _ ->
    !! @"src/KitchenSink/project.json"
      |> DotNetCli.Build (fun p ->
          {p with
            Configuration = "Release"})
)

Target "CompileTests" (fun _ ->
    !! @"src/KitchenSink.Tests/project.json"
      |> DotNetCli.Build (fun p ->
          {p with
            Configuration = "Release"})
)

Target "RunTests" (fun _ ->
    !! @"src/KitchenSink.Tests/project.json"
      |> DotNetCli.Test (fun p ->
        { p with
            Configuration = "Release"})
)

Target "FxCop" (fun _ ->
    !! (outputDir @@ @"/**/*.dll")
      ++ (outputDir + @"/**/*.exe")
        |> FxCop (fun p ->
            {p with
                ReportFileName = testDir + "FXCopResults.xml";
                ToolPath = fxCopExe})
)

Target "Release" (fun _ ->
    if gitVersion.BuildMetaData = "" then log "~~~ !!! RELEASE TIME !!! ~~~"
    //Set an env variable to let build server know to deploy !??
)

// Dependencies
"Clean"
  ==> "SetVersions"
  ==> "CompileApp"
  //==> "FxCop"
  ==> "CompileTests"
  ==> "RunTests"
  ==> "Release"

// start build
RunTargetOrDefault "Release"

//match buildServer with
//    | AppVeyor -> "test"
//    | _ -> ""
