module Build

open Fake.Core
open Fake.DotNet
open Fake.Tools
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.Api


System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
let nupkgDir = __SOURCE_DIRECTORY__ </> "nupkg"

let gitOwner = "SaturnFramework"
let gitHome = "https://github.com/" + gitOwner
let gitName = "Saturn.Template"

let release = ReleaseNotes.parse (System.IO.File.ReadAllLines $"{__SOURCE_DIRECTORY__}/../RELEASE_NOTES.md")

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

let initializeContext args =
    let execContext = Context.FakeExecutionContext.Create false "build.fsx" args
    Context.setExecutionContext (Context.RuntimeContext.Fake execContext)

// --------------------------------------------------------------------------------------
// Build Targets
// --------------------------------------------------------------------------------------

let init args =
  initializeContext args
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
    ) $"{__SOURCE_DIRECTORY__}/../Saturn.Template.proj"
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

  let dependencies =
    [
      "Clean"
        ==> "Pack"
        ==> "ReleaseGitHub"
        ==> "Push"
        ==> "Release"
    ]

  ()

[<EntryPoint>]
let main args =
    init ((args |> List.ofArray))

    try
        match args with
        | [| target |] -> Target.runOrDefaultWithArguments target
        | _ -> Target.runOrDefaultWithArguments "Pack"
        0
    with
    | e ->
        printfn "%A" e
        1