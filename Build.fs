open Fake.Core
open Fake.IO
open Farmer
open Farmer.Builders

open Helpers
open System

initializeContext()

let sharedPath = Path.getFullName "src/Shared"
let serverPath = Path.getFullName "src/Server"
let clientPath = Path.getFullName "src/Client"
let deployPath = Path.getFullName "deploy"
let sharedTestsPath = Path.getFullName "tests/Shared"
let serverTestsPath = Path.getFullName "tests/Server"
let clientTestsPath = Path.getFullName "tests/Client"

let url = "https://localhost:3000"

Target.create "Clean" (fun _ ->
    Shell.cleanDir deployPath
    run dotnet "fable clean --yes" clientPath // Delete *.fs.js files created by Fable
)

Target.create "InstallClient" (fun _ -> run npm "install" ".")

Target.create "InstallOfficeAddinTooling" (fun _ ->

    printfn "Installing office addin tooling"

    run npm "install -g office-addin-dev-certs" __SOURCE_DIRECTORY__
    run npm "install -g office-addin-debugging" __SOURCE_DIRECTORY__
    run npm "install -g office-addin-manifest" __SOURCE_DIRECTORY__
)

Target.create "WebpackConfigSetup" (fun _ ->
    let userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)

    Shell.replaceInFiles
        [
            "{USERFOLDER}",userPath.Replace("\\","/")
        ]
        [
            (Path.combine __SOURCE_DIRECTORY__ "webpack.config.js")
        ]
)

Target.create "SetLoopbackExempt" (fun _ ->
    Command.RawCommand("CheckNetIsolation.exe",Arguments.ofList [
        "LoopbackExempt"
        "-a"
        "-n=\"microsoft.win32webviewhost_cw5n1h2txyewy\""
    ])
    |> CreateProcess.fromCommand
    |> Proc.run
    |> ignore
)

Target.create "CreateDevCerts" (fun _ ->
    run npx "office-addin-dev-certs install --days 365" __SOURCE_DIRECTORY__

    let certPath =
        Path.combine
            (Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
            ".office-addin-dev-certs/ca.crt"
        

    let psi = new System.Diagnostics.ProcessStartInfo(FileName = certPath, UseShellExecute = true)
    System.Diagnostics.Process.Start(psi) |> ignore
)

Target.create "officedebug" (fun _ ->
    run dotnet "build" sharedPath
    openBrowser url
    [ "server", dotnet "watch run" serverPath
      "client", dotnet "fable watch src/Client -s --run webpack-dev-server" ""
      /// sideload webapp in excel
      "officedebug", npx "office-addin-debugging start manifest.xml desktop --debug-method web" __SOURCE_DIRECTORY__
      ]
    |> runParallel
)

Target.create "Bundle" (fun _ ->
    [ "server", dotnet $"publish -c Release -o \"{deployPath}\"" serverPath
      //"client", dotnet "fable -o output -s --run webpack -p" ""
      "client",
        CreateProcess.fromRawCommandLine "dotnet" "fable src/Client -s --run webpack --mode production"
        |> CreateProcess.ensureExitCode
      ]
    |> runParallel
)

Target.create "Azure" (fun _ ->
    let web = webApp {
        name "NewSwate_"
        zip_deploy "deploy"
    }
    let deployment = arm {
        location Location.WestEurope
        add_resource web
    }

    deployment
    |> Deploy.execute "NewSwate_" Deploy.NoParameters
    |> ignore
)


Target.create "Run" (fun _ ->
    run dotnet "build" sharedPath
    openBrowser url
    [ "server", dotnet "watch run" serverPath
      //"client", dotnet "fable watch src/Client --run webpack serve --mode development" "" // Working
      "client", dotnet "fable watch src/Client -s --run webpack-dev-server" ""
    ]
    |> runParallel
)

Target.create "RunTests" (fun _ ->
    run dotnet "build" sharedTestsPath
    [ "server", dotnet "watch run" serverTestsPath
      "client", dotnet "fable watch -o output -s --run webpack-dev-server --config ../../webpack.tests.config.js" clientTestsPath ]
    |> runParallel
)

Target.create "Format" (fun _ ->
    run dotnet "fantomas . -r" "src"
)

Target.create "Setup" ignore 

open Fake.Core.TargetOperators

let dependencies = [
    "Clean"
        ==> "InstallClient"
        ==> "Bundle"
        ==> "Azure"

    "Clean"
        ==> "InstallClient"
        ==> "Run"

    "InstallClient"
        ==> "RunTests"

    "InstallOfficeAddinTooling"
        ==> "WebpackConfigSetup"
        ==> "CreateDevCerts"
        ==> "SetLoopbackExempt"
        ==> "Setup"

    "Clean"
        ==> "InstallClient"
        ==> "officedebug"
]

[<EntryPoint>]
let main args = runOrDefault args