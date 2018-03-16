namespace DemoApplication.Interop

[<Struct>]
type WNDCLASSEX =
    val mutable public cbSize:UINT
    val mutable public style:UINT
    val mutable public lpfnWndProc:nativeint
    val mutable public cbClsExtra:int32
    val mutable public cbWndExtra:int32
    val mutable public hInstance:HINSTANCE
    val mutable public hIcon:HICON
    val mutable public hCursor:HCURSOR
    val mutable public hbrBackground:HBRUSH
    val mutable public lpszMenuName:LPCWSTR
    val mutable public lpszClassName:LPCWSTR
    val mutable public hIconSm:HICON
