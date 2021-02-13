#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open System.Threading

open System.Runtime.InteropServices

let appPath = "./src/SaturnServer/" |> Path.getFullName
let projectPath = Path.combine appPath "SaturnServer.fsproj"


Target.create "Clean" ignore

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

    if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
      CreateProcess.fromRawCommand "cmd.exe" [ "/C"; "start http://localhost:8085" ] |> Proc.run |> ignore
    elif RuntimeInformation.IsOSPlatform(OSPlatform.Linux) then
      CreateProcess.fromRawCommand "xdg-open" [ "http://localhost:8085" ] |> Proc.run |> ignore
    elif RuntimeInformation.IsOSPlatform(OSPlatform.OSX) then
      CreateProcess.fromRawCommand "open" [ "http://localhost:8085" ] |> Proc.run |> ignore
  }

  [ server; browser]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore
)

"Clean"
  ==> "Restore"
  ==> "Build"

"Clean"
  ==> "Restore"
  ==> "Run"

Target.runOrDefault "Build"