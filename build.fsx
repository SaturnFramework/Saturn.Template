// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------
#r "paket: groupref build //"
#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.Tools
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.Api


System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
let templatePath = "./Content/.template.config/template.json"
let templateName = "Saturn"
let nupkgDir = __SOURCE_DIRECTORY__ </> "nupkg"

let gitOwner = "SaturnFramework"
let gitHome = "https://github.com/" + gitOwner
let gitName = "Saturn.Template"

let gitUrl = gitHome + "/" + gitName

let release = ReleaseNotes.parse (System.IO.File.ReadAllLines "RELEASE_NOTES.md")

let formattedRN =
  release.Notes
  |> List.map (sprintf "* %s")
  |> String.concat "\n"

// --------------------------------------------------------------------------------------
// Helpers
// --------------------------------------------------------------------------------------
let isNullOrWhiteSpace = System.String.IsNullOrWhiteSpace

let getBuildParam = Environment.environVar

let DoNothing = ignore

// --------------------------------------------------------------------------------------
// Build Targets
// --------------------------------------------------------------------------------------


Target.create "Clean" (fun _ ->
    Shell.cleanDirs [nupkgDir]
)

Target.create  "Pack" (fun _ ->
  Environment.setEnvironVar "Version" release.NugetVersion
  Environment.setEnvironVar "PackageVersion" release.NugetVersion
  Environment.setEnvironVar "PackageReleaseNotes" formattedRN

  DotNet.pack ( fun args ->
    { args with
        OutputPath = Some nupkgDir
    }
  ) ""
)

Target.create "ReleaseGitHub" (fun _ ->
  let remote =
      Git.CommandHelper.getGitResult "" "remote -v"
      |> Seq.filter (fun (s: string) -> s.EndsWith("(push)"))
      |> Seq.tryFind (fun (s: string) -> s.Contains(gitOwner + "/" + gitName))
      |> function None -> gitHome + "/" + gitName | Some (s: string) -> s.Split().[0]

  Git.Staging.stageAll ""
  Git.Commit.exec "" (sprintf "Bump version to %s" release.NugetVersion)
  Git.Branches.pushBranch "" remote "master"


  Git.Branches.tag "" release.NugetVersion
  Git.Branches.pushTag "" remote release.NugetVersion

  let client =
      let user =
          match getBuildParam "github-user" with
          | s when not (isNullOrWhiteSpace s) -> s
          | _ -> UserInput.getUserInput "Username: "
      let pw =
          match getBuildParam "github-pw" with
          | s when not (isNullOrWhiteSpace s) -> s
          | _ -> UserInput.getUserPassword "Password: "

      // Git.createClient user pw
      GitHub.createClient user pw
  let files = !! (nupkgDir </> "*.nupkg")

  // release on github
  let cl =
      client
      |> GitHub.draftNewRelease gitOwner gitName release.NugetVersion (release.SemVer.PreRelease <> None) release.Notes
  (cl,files)
  ||> Seq.fold (fun acc e -> acc |> GitHub.uploadFile e)
  |> GitHub.publishDraft//releaseDraft
  |> Async.RunSynchronously
)

Target.create "Push" (fun _ ->
    let key =
        match getBuildParam "nuget-key" with
        | s when not (isNullOrWhiteSpace s) -> s
        | _ -> UserInput.getUserPassword "NuGet Key: "
    Paket.push (fun p -> { p with WorkingDir = nupkgDir; ApiKey = key; ToolType = ToolType.CreateLocalTool() }))


Target.create "Release" DoNothing

"Clean"
  ==> "Pack"
  ==> "ReleaseGitHub"
  ==> "Push"
  ==> "Release"

Target.runOrDefault "Pack"