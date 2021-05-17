module ExampleProvider

open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes
open System.Reflection

[<TypeProvider>]
type ExampleProvider(config : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces(config)
    let namespaceName = "ExampleProvider"
    let thisAssembly = Assembly.GetExecutingAssembly()
    let t = ProvidedTypeDefinition(
                thisAssembly,
                namespaceName,
                className = "ProvidedType",
                baseType = Some typeof<obj>)
    let staticProp =
        ProvidedProperty(
            propertyName = "StaticProperty",
            propertyType = typeof<string>,
            isStatic = true,
            getterCode = (fun _ -> <@@ "Hello!" @@>))
    do t.AddMember staticProp
    do this.AddNamespace(namespaceName, [t])

[<assembly:TypeProviderAssembly>]
do()
