namespace DemoApplication.Interop

[<Struct>]
type CREATESTRUCT =
    val public lpCreateParams:LPVOID
    val public hInstance:HINSTANCE
    val public hMenu:HMENU
    val public hwndParent:HWND
    val public cy:int32
    val public cx:int32
    val public y:int32
    val public x:int32
    val public style:LONG
    val public lpszName:LPCWSTR
    val public lpszClass:LPCWSTR
    val public dwExStyle:DWORD
