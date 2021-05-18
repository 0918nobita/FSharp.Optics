module Program

// open Microsoft.FSharp.Reflection
open System
open System.Linq.Expressions
open System.Reflection
open System.Reflection.Emit

type User = User of id : int * name : string

type CSharpFunc =
    static member ToFSharpFunc<'a, 'b>(func : Func<'a, 'b>) : 'a -> 'b =
        fun a -> func.Invoke(a)
    static member ToFSharpFunc<'a, 'b, 'c>(func : Func<'a, 'b, 'c>) : 'a -> 'b -> 'c =
        fun a b -> func.Invoke(a, b)

[<EntryPoint>]
let main _ =
    let intType = typeof<int>
    let method = DynamicMethod(
                    name = "add",
                    returnType = intType,
                    parameterTypes = [| intType; intType |])

    let gen = method.GetILGenerator()
    gen.Emit OpCodes.Ldarg_0
    gen.Emit OpCodes.Ldarg_1
    gen.Emit OpCodes.Add
    gen.Emit OpCodes.Ret

    let addIL =
        method.CreateDelegate(typeof<Func<int, int, int>>) :?> Func<int, int, int>
        |> CSharpFunc.ToFSharpFunc

    let paramX = Expression.Parameter(intType, "x")
    let paramY = Expression.Parameter(intType, "y")
    let lambda = Expression.Lambda(Expression.Add(paramX, paramY), [paramX; paramY])
    let addE =
        lambda.Compile() :?> Func<int, int, int>
        |> CSharpFunc.ToFSharpFunc

    printfn "%d" <| addIL 1 2 // => 3
    printfn "%d" <| addE 1 2  // => 3

    let methodInfo : MethodInfo = downcast typeof<User>.GetMember("NewUser").[0]
    let paramName = Expression.Parameter(typeof<string>, "name")
    let expr =
        Expression.Lambda(
            Expression.Call(
                methodInfo,
                [|
                    Expression.Constant 21 :> Expression
                    paramName :> Expression
                |]),
            [| paramName |])
    let createUser =
        expr.Compile() :?> Func<string, User>
        |> CSharpFunc.ToFSharpFunc

    printfn "%A" <| createUser "Alice"
    0
