namespace DemoApplication.Interop

open System.Runtime.InteropServices

module Gdi32 =
    // BI_* Constants
    [<Literal>]
    let public BI_RGB:DWORD = 0u

    // DIB_* Constants
    [<Literal>]
    let public DIB_RGB_COLORS:DWORD = 0u

    // Raster Operation Constants
    [<Literal>]
    let public SRCCOPY:DWORD = 0x00CC0020u
