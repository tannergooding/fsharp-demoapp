namespace DemoApplication.Interop

[<Struct>]
type MSG =
    val public hwnd:HWND
    val public message:UINT
    val public wParam:WPARAM
    val public lParam:LPARAM
    val public time:DWORD
    val public pt:POINT
