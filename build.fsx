#r @"packages/FAKE/tools/FakeLib.dll"
#load "paket-files/fsharp/FAKE/modules/Octokit/Octokit.fsx"

open System
open Fake
open Fake.ReleaseNotesHelper
open Octokit

let templatePath = "./Content/.template.config/template.json"
let templateName = "Saturn"
let nupkgDir = FullName "./nupkg"

let release = LoadReleaseNotes "RELEASE_NOTES.md"

let formattedRN =
  release.Notes
  |> List.map (sprintf "* %s")
  |> String.concat "\n"

Target "Clean" (fun () ->
  CleanDirs [
    nupkgDir
  ]
)

Target "Pack" (fun () ->
  RegexReplaceInFileWithEncoding
    "  \"name\": .+,"
   ("  \"name\": \"" + templateName + " v" + release.NugetVersion + "\",")
    System.Text.Encoding.UTF8
    templatePath
  DotNetCli.Pack ( fun args ->
    { args with
        OutputPath = nupkgDir
        AdditionalArgs =
          [
            sprintf "/p:PackageVersion=%s" release.NugetVersion
            sprintf "/p:PackageReleaseNotes=\"%s\"" formattedRN
          ]
    }
  )
)

Target "ReleaseGitHub" (fun _ ->

  let remoteGit = "origin"
  let commitMsg = sprintf "Bumping version to %O" release.NugetVersion
  let tagName = string release.NugetVersion
  let gitOwner = "Krzysztof-Cieslak"
  let gitName = "Saturn.Template"


  Git.Branches.checkout "" false "master"
  Git.CommandHelper.directRunGitCommand "" "fetch origin" |> ignore
  Git.CommandHelper.directRunGitCommand "" "fetch origin --tags" |> ignore

  Git.Staging.StageAll ""
  Git.Commit.Commit "" commitMsg
  Git.Branches.pushBranch "" remoteGit "master"

  Git.Branches.tag "" tagName
  Git.Branches.pushTag "" remoteGit tagName

  let client =
    let user =
        match getBuildParam "github-user" with
        | s when not (String.IsNullOrWhiteSpace s) -> s
        | _ -> getUserInput "Username: "
    let pw =
        match getBuildParam "github-pw" with
        | s when not (String.IsNullOrWhiteSpace s) -> s
        | _ -> getUserPassword "Password: "

    createClient user pw

  // release on github
  client
  |> createDraft gitOwner gitName release.NugetVersion (release.SemVer.PreRelease <> None) release.Notes
  |> releaseDraft
  |> Async.RunSynchronously

)

Target "Push" (fun () ->
    let key =
        match getBuildParam "nuget-key" with
        | s when not (String.IsNullOrWhiteSpace s) -> s
        | _ -> getUserPassword "NuGet Key: "
    Paket.Push (fun p -> { p with WorkingDir = nupkgDir; ApiKey = key }))

Target "Release" DoNothing

"Clean"
  ==> "Pack"
  ==> "ReleaseGitHub"
  ==> "Push"
  ==> "Release"

RunTargetOrDefault "Pack"