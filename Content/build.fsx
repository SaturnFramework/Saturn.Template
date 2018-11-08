#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open System.Threading

let appPath = "./src/SaturnServer/" |> Path.getFullName
let projectPath = Path.combine appPath "SaturnServer.fsproj"
let dotnetcliVersion = DotNet.getSDKVersionFromGlobalJson()


Target.create "Clean" ignore

Target.create "InstallDotNetCore" (fun _ ->
  DotNet.install
    (fun o -> { o with Version = DotNet.CliVersion.Version dotnetcliVersion })
  |> ignore
)

Target.create "Restore" (fun _ ->
    DotNet.restore id projectPath
)

Target.create "Build" (fun _ ->
    DotNet.build id projectPath
)


Target.create "Run" (fun _ ->
  let server = async {
    DotNet.exec (fun p -> { p with WorkingDirectory = appPath } ) "watch" "run" |> ignore
  }
  let browser = async {
    Thread.Sleep 5000
    Process.start (fun i -> { i with FileName = "http://localhost:8085" }) |> ignore
  }

  [ server; browser]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore
)

"Clean"
  ==> "InstallDotNetCore"
  ==> "Build"

"Clean"
  ==> "Restore"
  ==> "Run"

Target.runOrDefault "Build"