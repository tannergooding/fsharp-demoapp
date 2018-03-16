namespace DemoApplication.Interop

open System.Runtime.InteropServices

module User32 =
    // COLOR_* Constants
    [<Literal>]
    let public COLOR_WINDOW:int32 = 5

    // CS_* Constants
    [<Literal>]
    let public CS_VREDRAW:UINT = 0x0001u

    [<Literal>]
    let public CS_HREDRAW:UINT = 0x0002u

    [<Literal>]
    let public CS_DBLCLKS:UINT = 0x0008u

    [<Literal>]
    let public CS_OWNDC:UINT = 0x0020u

    // CW_* Constants
    [<Literal>]
    let public CW_USEDEFAULT:int32 = 0x80000000

    // GWLP_* Constants
    [<Literal>]
    let public GWLP_USERDATA:int32 = -21

    // PM_* Constants
    [<Literal>]
    let public PM_REMOVE:UINT = 0x0001u

    // RDW_* Constants
    [<Literal>]
    let public RDW_INTERNALPAINT:UINT = 0x0002u

    [<Literal>]
    let public RDW_UPDATENOW:UINT = 0x0100u

    // SW_* Constants
    [<Literal>]
    let public SW_HIDE:int32 = 0

    [<Literal>]
    let public SW_SHOW:int32 = 5

    // WA_* Constants
    [<Literal>]
    let public WA_INACTIVE:uint16 = 0us

    // WM_* Constants
    [<Literal>]
    let public WM_NULL:UINT = 0x0000u

    [<Literal>]
    let public WM_CREATE:UINT = 0x0001u

    [<Literal>]
    let public WM_DESTROY:UINT = 0x0002u

    [<Literal>]
    let public WM_MOVE:UINT = 0x0003u

    [<Literal>]
    let public WM_SIZE:UINT = 0x0005u

    [<Literal>]
    let public WM_ACTIVATE:UINT = 0x0006u

    [<Literal>]
    let public WM_SETTEXT:UINT = 0x000Cu

    [<Literal>]
    let public WM_PAINT:UINT = 0x000Fu

    [<Literal>]
    let public WM_CLOSE:UINT = 0x0010u

    [<Literal>]
    let public WM_QUIT:UINT = 0x0012u

    [<Literal>]
    let public WM_ERASEBKGND:UINT = 0x0014u

    [<Literal>]
    let public WM_SHOWWINDOW:UINT = 0x0018u

    // WS_* Constants
    [<Literal>]
    let public WS_OVERLAPPED:DWORD = 0x00000000u

    [<Literal>]
    let public WS_CAPTION:DWORD = 0x00C00000u

    [<Literal>]
    let public WS_SYSMENU:DWORD = 0x00080000u

    [<Literal>]
    let public WS_THICKFRAME:DWORD = 0x00040000u

    [<Literal>]
    let public WS_MINIMIZEBOX:DWORD = 0x00020000u

    [<Literal>]
    let public WS_MAXIMIZEBOX:DWORD = 0x00010000u

    [<Literal>]
    let public WS_OVERLAPPEDWINDOW:DWORD = (WS_OVERLAPPED ||| WS_CAPTION ||| WS_SYSMENU ||| WS_THICKFRAME ||| WS_MINIMIZEBOX ||| WS_MAXIMIZEBOX)

    // WS_EX_* Constants
    [<Literal>]
    let public WS_EX_WINDOWEDGE:DWORD = 0x0100u

    [<Literal>]
    let public WS_EX_CLIENTEDGE:DWORD = 0x0200u

    [<Literal>]
    let public WS_EX_OVERLAPPEDWINDOW:DWORD = (WS_EX_WINDOWEDGE ||| WS_EX_CLIENTEDGE)

    // External Methods
    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "CreateWindowExW", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern HWND CreateWindowEx(
        [<In>] DWORD dwExStyle,
        [<In; Optional>] LPCWSTR lpClassName,
        [<In; Optional>] LPCWSTR lpWindowName,
        [<In>] DWORD dwStyle,
        [<In>] int32 X,
        [<In>] int32 Y,
        [<In>] int32 nWidth,
        [<In>] int32 nHeight,
        [<In; Optional>] HWND hWndParent,
        [<In; Optional>] HMENU hMenu,
        [<In; Optional>] HINSTANCE hInstance,
        [<In>] LPVOID lpParam
    )

    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "DefWindowProcW", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern LRESULT DefWindowProc(
        [<In>] HWND hWnd,
        [<In>] UINT msg,
        [<In>] WPARAM wParam,
        [<In>] LPARAM lParam
    )

    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "DestroyWindow", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern BOOL DestroyWindow(
        [<In>] HWND hWnd
    )

    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "DispatchMessageW", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern LRESULT DispatchMessage(
        [<In>] MSG& lpMsg
    )

    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "GetActiveWindow", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern HWND GetActiveWindow()

    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "GetClassInfoExW", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern BOOL GetClassInfoEx(
        [<In; Optional>] HINSTANCE hInstance,
        [<In>] LPCWSTR lpszClass,
        [<Out>] WNDCLASSEX& lpwcx
    )

    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "GetClassNameW", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern int32 GetClassName(
        [<In>] HWND hWnd,
        [<Out>] LPWSTR lpClassName,
        [<In>] int32 nMaxCount
    )

    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "GetDC", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern HDC GetDC(
        [<In>] HWND hWnd
    )

    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "GetDesktopWindow", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern HWND GetDesktopWindow()

    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "GetWindowLongPtrW", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern LONG_PTR GetWindowLongPtr(
        [<In>] HWND hWnd,
        [<In>] int32 nIndex
    )

    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "GetWindowRect", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern BOOL GetWindowRect(
        [<In>] HWND hWnd,
        [<Out>] RECT& lpRect
    )

    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "PostQuitMessage", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern void PostQuitMessage(
        [<In>] int32 nExitCode
    )
    

    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "PeekMessageW", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern BOOL PeekMessage(
        [<Out>] MSG& lpMsg,
        [<In; Optional>] HWND hWnd,
        [<In>] UINT wMsgFilterMin,
        [<In>] UINT wMsgFilterMax,
        [<In>] UINT wRemoveMsg
    )

    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "RedrawWindow", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern BOOL RedrawWindow(
        [<In; Optional>] HWND hWnd,
        [<In; Optional>] RECT* lprcUpdate,
        [<In; Optional>] HRGN hrgnUpdate,
        [<In>] UINT flags
    )

    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "RegisterClassExW", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern ATOM RegisterClassEx(
        [<In>] WNDCLASSEX& lpWndClassEx
    )

    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "ReleaseDC", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern BOOL ReleaseDC(
        [<In>] HWND hWnd,
        [<In>] HDC hDC
    )

    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "SendMessageW", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern LRESULT SendMessage(
        [<In>] HWND hWnd,
        [<In>] UINT msg,
        [<In>] WPARAM wParam,
        [<In>] LPARAM lParam
    )

    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "SetForegroundWindow", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern BOOL SetForegroundWindow(
        [<In>] HWND hWnd
    )

    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "SetWindowLongPtrW", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern LONG_PTR SetWindowLongPtr(
        [<In>] HWND hWnd,
        [<In>] int32 nIndex,
        [<In>] LONG_PTR dwNewLong
    )

    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "ShowWindow", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern BOOL ShowWindow(
        [<In>] HWND hWnd,
        [<In>] int32 nCmdShow
    )

    [<DllImport("User32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "UnregisterClassW", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern BOOL UnregisterClass(
        [<In>] LPCWSTR lpClassName,
        [<In; Optional>] HINSTANCE hInstance
    )
