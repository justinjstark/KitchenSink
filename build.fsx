// include Fake lib
#r @"build/packages/FAKE/tools/FakeLib.dll"
#r @"build/packages/Newtonsoft.Json/lib/portable-net45+wp80+win8+wpa81/Newtonsoft.Json.dll"

open Fake
open Fake.Testing.NUnit3
open Fake.NuGet.Install
open Fake.AssemblyInfoFile
open Newtonsoft.Json

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

//let version2 = GitVersionHelper.GitVersion (fun p ->
//    { p with ToolPath = findToolInSubPath "GitVersion.exe" (packagesDir @@ "GitVersion.CommandLine") } //packagesDir @@ "/GitVersion.CommandLine/tools/GitVersion.exe"}
//)
//log ("GitVersion: " + version2)

//Workaround https://github.com/fsharp/FAKE/pull/1365
type GitVersionProperties = {
    Major : int;
    Minor : int;
    Patch : int;
    PreReleaseTag : string;
    PreReleaseTagWithDash : string;
    PreReleaseLabel : string;
    PreReleaseNumber : System.Nullable<int>;
    BuildMetaData : string;
    BuildMetaDataPadded : string;
    FullBuildMetaData : string;
    MajorMinorPatch : string;
    SemVer : string;
    LegacySemVer : string;
    LegacySemVerPadded : string;
    AssemblySemVer : string;
    FullSemVer : string;
    InformationalVersion : string;
    BranchName : string;
    Sha : string;
    NuGetVersionV2 : string;
    NuGetVersion : string;
    CommitsSinceVersionSource : int;
    CommitsSinceVersionSourcePadded : string;
    CommitDate : string;
}

let myGitVersionHelper = 
    let timespan =  System.TimeSpan.FromMinutes 1.
    let gitVersionToolPath = gitVersionExe
    let result = ExecProcessAndReturnMessages (fun info ->
        info.FileName <- gitVersionToolPath) timespan
    if result.ExitCode <> 0 then failwithf "GitVersion.exe failed with exit code %i" result.ExitCode
    result.Messages |> String.concat "" |> fun j -> JsonConvert.DeserializeObject<GitVersionProperties>(j)

let gitVersion = myGitVersionHelper
log ("GitVersion: " + gitVersion.FullSemVer)

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
    !! @"src/**/*.csproj"
      |> MSBuildRelease outputDir "Build"
      |> Log "AppBuild-Output: "
)

Target "CompileTests" (fun _ ->
    !! @"src/**/*.Tests.csproj"
      |> MSBuildDebug testDir "Build"
      |> Log "TestBuild-Output: "
)

Target "RunTests" (fun _ ->
    !! (testDir @@ @"/**/**.Tests.dll")
      |> NUnit (fun p ->
                 {p with
                   DisableShadowCopy = true;
                   OutputFile = testDir + @"TestResults.xml"})
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
  //==> "CompileApp"
  //==> "FxCop"
  //==> "CompileTests"
  //==> "RunTests"
  ==> "Release"

// start build
RunTargetOrDefault "Release"

//match buildServer with
//    | AppVeyor -> "test"
//    | _ -> ""
