namespace DemoApplication.Interop

module Windows =
    // Constants
    [<Literal>]
    let public FALSE:BOOL = 0

    [<Literal>]
    let public TRUE:BOOL = 1

    [<Literal>]
    let public NULL:int32 = 0

    // Methods
    let LOWORD(value:unativeint) : uint16 = uint16(value &&& 0xFFFFun)

    let HIWORD(value:unativeint) : uint16 = LOWORD(value >>> 16)
