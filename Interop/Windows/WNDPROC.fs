namespace DemoApplication.Interop

open System.Runtime.InteropServices

[<UnmanagedFunctionPointer(CallingConvention.ThisCall, BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = false, ThrowOnUnmappableChar = false)>]
type WNDPROC = delegate of hWnd:HWND * msg:UINT * wParam:WPARAM * lParam:LPARAM -> LRESULT
