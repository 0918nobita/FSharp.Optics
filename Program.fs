module Program

open System.Reflection
open System.Reflection.Emit

[<EntryPoint>]
let main _ =
    let asmName = AssemblyName("DynAsm")

    let asmBuilder =
        AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndCollect)
    let modBuilder = asmBuilder.DefineDynamicModule(asmName.Name)
    let typeBuilder = modBuilder.DefineType("MyDynamicType", TypeAttributes.Public)
    let intType = typeof<int>
    let methodBuilder =
        typeBuilder.DefineMethod(
            "addFunc",
            MethodAttributes.Public ||| MethodAttributes.Static,
            returnType = intType,
            parameterTypes = [| intType; intType |])

    let ilGen = methodBuilder.GetILGenerator()
    ilGen.Emit OpCodes.Ldarg_0
    ilGen.Emit OpCodes.Ldarg_1
    ilGen.Emit OpCodes.Add
    ilGen.Emit OpCodes.Ret

    ignore <| typeBuilder.CreateType()

    let asmGen = Lokad.ILPack.AssemblyGenerator()
    asmGen.GenerateAssembly(asmBuilder, asmName.Name + ".dll")
    0
