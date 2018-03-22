namespace DemoApplication

open System.Globalization
open System.Resources

[<AbstractClass; Sealed>]
type Resources private () =
    // Static Properties
    static member public ArgumentNullExceptionMessage with get() : string = Resources.GetString "ArgumentNullExceptionMessage"

    static member public ArgumentOutOfRangeExceptionMessage with get() : string = Resources.GetString "ArgumentOutOfRangeExceptionMessage"

    static member val public Culture:CultureInfo = null with get, set

    static member public ExternalExceptionMessage with get() : string = Resources.GetString "ExternalExceptionMessage"

    static member public InvalidOperationExceptionMessage with get() : string = Resources.GetString "InvalidOperationExceptionMessage"

    static member val public ResourceManager:ResourceManager = new ResourceManager(typeof<Resources>)

    // Static Methods
    static member private GetString(name:string) : string =
        Resources.ResourceManager.GetString(name, Resources.Culture)
