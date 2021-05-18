module Program

open System.Reflection.Emit

type Delegate1 = delegate of int * int -> int

[<EntryPoint>]
let main _ =
    let intType = typeof<int>
    let method = DynamicMethod(
                    name = "add",
                    returnType = intType,
                    parameterTypes = [| intType; intType |])

    let ilGenerator = method.GetILGenerator()
    ilGenerator.Emit OpCodes.Ldarg_0
    ilGenerator.Emit OpCodes.Ldarg_1
    ilGenerator.Emit OpCodes.Add
    ilGenerator.Emit OpCodes.Ret

    let addDelegate : Delegate1 =
        downcast method.CreateDelegate(delegateType = typeof<Delegate1>)

    addDelegate.Invoke(1, 2)
    |> printfn "%d" // => 3
    0
