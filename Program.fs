module Program

open System
open System.IO
open FSharp.Compiler.SourceCodeServices

type FsProject = FsProject of fsproj : string * src : string

let genFsProject () : FsProject =
    let base1 = Path.GetTempFileName()
    let srcFileName = Path.ChangeExtension(base1, ".fs")
    let srcFileContent = """namespace Foo

module Inner =
    let rec fact n = if n > 0 then n * (fact (n - 1)) else 1

type User = User of id : int * name : string

type Message = Message of User * string

namespace Bar

type Color = { R : int; G : int; B : int }

type Hoge =
    member this.Fuga(s : string) = s + "!"
"""
    File.WriteAllText(srcFileName, srcFileContent)
    let base2 = Path.GetTempFileName()
    let projFileName = Path.ChangeExtension(base2, ".fsproj")
    FsProject (projFileName, srcFileName)

let (|FsModule|_|) (entity : FSharpEntity) =
    if entity.IsFSharpModule then Some () else None

let (|FsUnion|_|) (entity : FSharpEntity) =
    if entity.IsFSharpUnion then Some () else None

let (|FsRecord|_|) (entity : FSharpEntity) =
    if entity.IsFSharpRecord then Some () else None

[<EntryPoint>]
let main _ =
    let (FsProject (fsproj, src)) = genFsProject ()

    let checker = FSharpChecker.Create()

    let projOptions =
        let runtimeDir = Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()
        let baseDir = AppContext.BaseDirectory
        let lib name = Path.Combine(runtimeDir, name)
        checker.GetProjectOptionsFromCommandLineArgs(
            fsproj,
            [|
                "--noframework"
                "--target:library"
                src
                "-r:" + lib "mscorlib.dll"
                "-r:" + lib "System.dll"
                "-r:" + lib "System.Collections.dll"
                "-r:" + lib "System.Core.dll"
                "-r:" + lib "System.Net.Requests.dll"
                "-r:" + lib "System.Net.WebClient.dll"
                "-r:" + lib "System.Private.CoreLib.dll"
                "-r:" + lib "System.Runtime.dll"
                "-r:" + lib "System.Runtime.Numerics.dll"
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

    let unionCaseToString (unionCase : FSharpUnionCase) : string =
        sprintf
            "%s of %s"
            unionCase.Name
            (unionCase.UnionCaseFields
            |> Seq.map (fun item ->
                sprintf
                    "%s : %s"
                    item.Name
                    (item.FieldType.Format FSharpDisplayContext.Empty))
            |> String.concat " * ")

    wholeProjResults.AssemblySignature.Entities
    |> Seq.iter (fun entity ->
        match entity with
        | FsModule ->
            printfn "module %s" entity.CompiledName
        | FsUnion ->
            printfn "union %s" entity.CompiledName
            entity.UnionCases
            |> Seq.map unionCaseToString
            |> Seq.iter (printfn "    %s")
        | FsRecord ->
            printfn "record %s" entity.CompiledName
        | _ ->
            printfn "type %s" entity.CompiledName

        entity.MembersFunctionsAndValues
        |> Seq.iter (printfn "    %A")
        printfn "")
    0
