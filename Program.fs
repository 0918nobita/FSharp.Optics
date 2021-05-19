module Program

open System
open FSharp.Reflection

let glue<'t> : 't =
    try failwith "glue code"
    with ex -> raise ex

type User = User of id : int * name : string

type DerivedLens<'s, 'a> =
    member _.Get : 's -> 'a = glue
    member _.Set : 's -> 'a -> 's = glue

type S<'n> = S of 'n
type Z     = Z

type S<'n> with
    static member inline (+^) (S x, y) = S (x +^ y)

type Z with
    static member inline (+^) (Z, y) = y

type CSharpFunc =
    static member ToFSharpFunc<'a, 'b>(func : Func<'a, 'b>) : 'a -> 'b = fun a -> func.Invoke(a)

[<EntryPoint>]
let main _ =
    let userType = typeof<User>
    let unionCase = Array.head <| FSharpType.GetUnionCases userType
    let field = Array.head <| unionCase.GetFields()
    let method = userType.GetProperty(field.Name).GetGetMethod()
    let func =
        Delegate.CreateDelegate(
            typeof<Func<User, int>>,
            method
        ) :?> Func<User, int>
        |> CSharpFunc.ToFSharpFunc

    let user = User (21, "Alice")
    for _ in 1..10 do
        printfn "%d" <| func user
    0
