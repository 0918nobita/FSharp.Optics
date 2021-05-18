module Program

open System
open System.IO
open FSharp.Compiler.SourceCodeServices

type FsProject = FsProject of fsproj : string * src : string

let genFsProject () : FsProject =
    let base1 = Path.GetTempFileName()
    let srcFileName = Path.ChangeExtension(base1, ".fs")
    let srcFileContent = """module Program

type User = User of id : int * name : string

let add x y = x + y
"""
    File.WriteAllText(srcFileName, srcFileContent)
    let base2 = Path.GetTempFileName()
    let projFileName = Path.ChangeExtension(base2, ".fsproj")
    FsProject (projFileName, srcFileName)

[<EntryPoint>]
let main _ =
    let (FsProject (fsproj, src)) = genFsProject ()

    let checker = FSharpChecker.Create()

    let projOptions =
        let runtimeDir = Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()
        let baseDir = AppContext.BaseDirectory
        checker.GetProjectOptionsFromCommandLineArgs(
            fsproj,
            [|
                "--noframework"
                "--target:library"
                src
                "-r:" + Path.Combine(runtimeDir, "mscorlib.dll")
                "-r:" + Path.Combine(runtimeDir, "System.dll")
                "-r:" + Path.Combine(runtimeDir, "System.Collections.dll")
                "-r:" + Path.Combine(runtimeDir, "System.Core.dll")
                "-r:" + Path.Combine(runtimeDir, "System.Net.Requests.dll")
                "-r:" + Path.Combine(runtimeDir, "System.Net.WebClient.dll")
                "-r:" + Path.Combine(runtimeDir, "System.Private.CoreLib.dll")
                "-r:" + Path.Combine(runtimeDir, "System.Runtime.dll")
                "-r:" + Path.Combine(runtimeDir, "System.Runtime.Numerics.dll")
                "-r:" + Path.Combine(baseDir, "FSharp.Core.dll")
            |])

    let wholeProjResults =
        checker.ParseAndCheckProject(projOptions)
        |> Async.RunSynchronously

    let errors = wholeProjResults.Errors
    if not (Array.isEmpty errors)
        then
            eprintfn "%A" <| errors
            exit 1

    wholeProjResults.AssemblySignature.Entities
    |> Seq.iter (fun x ->
        printfn "DisplayName: %s" x.DisplayName

        printfn "NestedEntries:"
        x.NestedEntities
        |> Seq.iter (fun y ->
            printfn "    %A" y)

        printfn "MembersFunctionsAndValues:"
        x.MembersFunctionsAndValues
        |> Seq.iter (fun y ->
            printfn "    %A" y))
    0
