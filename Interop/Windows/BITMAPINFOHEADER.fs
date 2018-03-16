namespace DemoApplication.Interop

[<Struct>]
type BITMAPINFOHEADER =
    val mutable public biSize:DWORD
    val mutable public biWidth:LONG
    val mutable public biHeight:LONG
    val mutable public biPlanes:WORD
    val mutable public biBitCount:WORD
    val mutable public biCompression:DWORD
    val mutable public biSizeImage:DWORD
    val mutable public biXPelsPerMeter:LONG
    val mutable public biYPelsPerMeter:LONG
    val mutable public biClrUsed:DWORD
    val mutable public biClrImportant:DWORD
