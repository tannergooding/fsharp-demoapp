namespace DemoApplication.Provider.Win32.UI

open DemoApplication
open DemoApplication.Interop
open DemoApplication.Provider.Win32.Graphics
open DemoApplication.UI
open DemoApplication.Utilities
open Microsoft.FSharp.NativeInterop
open System
open System.Threading
open System.Runtime.InteropServices

type Window internal (windowManager:IWindowManager, entryPointModule:HMODULE) =
    // Fields
    let paint:Event<EventHandler<PaintEventArgs>, PaintEventArgs> = Event<EventHandler<PaintEventArgs>, PaintEventArgs>()
    let windowManager:IWindowManager = windowManager
    let mutable title:string = typeof<Window>.FullName
    let parentThread:Thread = Thread.CurrentThread
    let hdd:HDRAWDIB = Msvfw32.DrawDibOpen()

    let handle:Lazy<HWND> = lazy (
        use windowName = fixed title
        let hWnd:HWND = User32.CreateWindowEx(User32.WS_EX_OVERLAPPEDWINDOW, NativePtr.ofNativeInt (windowManager.Handle), windowName, User32.WS_OVERLAPPEDWINDOW, User32.CW_USEDEFAULT, User32.CW_USEDEFAULT, User32.CW_USEDEFAULT, User32.CW_USEDEFAULT, HWND Windows.NULL, HMENU Windows.NULL, entryPointModule, HANDLE Windows.NULL)

        if hWnd = HWND Windows.NULL then
            ExceptionUtilities.ThrowExternalExceptionForLastError "CreateWindowEx"

        hWnd
    )

    let mutable bounds:Rectangle = Unchecked.defaultof<Rectangle>
    let flowDirection:FlowDirection = FlowDirection.TopToBottom
    let readingDirection:ReadingDirection = ReadingDirection.LeftToRight
    let mutable isActive:bool = false
    let mutable isVisible:bool = false

    // Events
    [<CLIEvent>]
    member public this.Paint:IEvent<EventHandler<PaintEventArgs>, PaintEventArgs> = paint.Publish

    // Properties
    member this.Bounds with get() : Rectangle = bounds
    member this.FlowDirection with get() : FlowDirection = flowDirection
    member this.Handle with get() : nativeint = handle.Value
    member this.IsActive with get() : bool = isActive
    member this.IsVisible with get() : bool = isVisible
    member this.ParentThread with get() : Thread = parentThread
    member this.ReadingDirection with get() : ReadingDirection = readingDirection
    member this.Title with get() : string = title
    member this.WindowManager with get() : IWindowManager = windowManager

    // Static Methods
    static member internal GetWindowBounds(handle:HWND) : Rectangle =
        let mutable rect:RECT = Unchecked.defaultof<RECT>

        if User32.GetWindowRect(handle, &rect) = Windows.FALSE then
            ExceptionUtilities.ThrowExternalExceptionForLastError "GetWindowRect"

        new Rectangle(float32 rect.left, float32 rect.top, float32 (rect.right - rect.left), float32 (rect.bottom - rect.top))

    static member internal IsWindowActive(handle:HWND) : bool =
        User32.GetActiveWindow() = handle

    // Methods
    member public this.Activate() : unit =
        ExceptionUtilities.ThrowIfNotThread parentThread

        if (not isActive) && (User32.SetForegroundWindow(this.Handle) = Windows.FALSE) then
            ExceptionUtilities.ThrowExternalExceptionForLastError "SetForegroundWindow"

    member public this.Close() : unit =
        if handle.IsValueCreated then
            User32.SendMessage(this.Handle, User32.WM_CLOSE, 0un, 0n) |> ignore

    member public this.Dispose() : unit =
        this.Dispose true
        GC.SuppressFinalize(this)

    member public this.Hide() : unit =
        ExceptionUtilities.ThrowIfNotThread parentThread

        if isVisible then
            User32.ShowWindow(this.Handle, User32.SW_HIDE) |> ignore

    member public this.Redraw() : unit =
        ExceptionUtilities.ThrowIfNotThread parentThread

        let hdc = User32.GetDC(this.Handle)

        let drawingContext = new DrawingContext(hdd, hdc)
        let eventArgs = new PaintEventArgs(drawingContext)
        paint.Trigger(this, eventArgs)

        User32.ReleaseDC(this.Handle, hdc) |> ignore

    member public this.Show() : unit =
        ExceptionUtilities.ThrowIfNotThread parentThread

        if not isVisible then
            User32.ShowWindow(this.Handle, User32.SW_SHOW) |> ignore

    member internal this.Dispose (isDisposing:bool) : unit =
        Msvfw32.DrawDibClose(hdd) |> ignore
        this.DisposeWindowHandle()

    member internal this.DisposeWindowHandle() : unit =
        if handle.IsValueCreated && (User32.DestroyWindow(this.Handle) = Windows.FALSE) then
            ExceptionUtilities.ThrowExternalExceptionForLastError "DestroyWindow"

    member internal this.HandleWmActivate(wParam:WPARAM) : LRESULT =
        isActive <- Windows.LOWORD(wParam) <> User32.WA_INACTIVE
        0n

    member internal this.HandleWmClose() : LRESULT =
        this.Dispose()
        0n

    member internal this.HandleWmDestroy() : LRESULT =
        this.HandleWmClose()

    member internal this.HandleWmEraseBkgnd() : LRESULT =
        0n

    member internal this.HandleWmMove(lParam:LPARAM) : LRESULT =
        bounds.Location <- new Point2D(float32(Windows.LOWORD(unativeint lParam)), float32(Windows.HIWORD(unativeint lParam)))
        0n

    member internal this.HandleWmSetText(wParam:WPARAM, lParam:LPARAM) : LRESULT =
        title <- Marshal.PtrToStringUni(lParam)
        User32.DefWindowProc(this.Handle, User32.WM_SETTEXT, wParam, lParam)

    member internal this.HandleWmShowWindow(wParam:WPARAM) : LRESULT =
        isVisible <- Windows.LOWORD(wParam) <> uint16 Windows.FALSE
        0n

    member internal this.HandleWmSize(lParam:LPARAM) : LRESULT =
        bounds.Size <- new Size2D(float32(Windows.LOWORD(unativeint lParam)), float32(Windows.HIWORD(unativeint lParam)))
        0n

    member internal this.ProcessWindowMessage(msg:UINT, wParam:WPARAM, lParam:LPARAM) : LRESULT =
        ExceptionUtilities.ThrowIfNotThread parentThread

        match msg with
        | User32.WM_DESTROY -> this.HandleWmDestroy()
        | User32.WM_MOVE -> this.HandleWmMove lParam
        | User32.WM_SIZE -> this.HandleWmSize lParam
        | User32.WM_ACTIVATE -> this.HandleWmActivate wParam
        | User32.WM_SETTEXT -> this.HandleWmSetText(wParam, lParam)
        | User32.WM_CLOSE -> this.HandleWmClose()
        | User32.WM_ERASEBKGND -> this.HandleWmEraseBkgnd()
        | User32.WM_SHOWWINDOW -> this.HandleWmShowWindow wParam
        | _ -> User32.DefWindowProc(this.Handle, msg, wParam, lParam)

    // System.IDisposable
    interface IDisposable with
        // Methods
        member this.Dispose() : unit = this.Dispose()

    // System.Object
    override this.Finalize() =
        this.Dispose false

    // DemoApplication.UI.IWindow
    interface IWindow with
        // Events
        [<CLIEvent>]
        member this.Paint:IEvent<EventHandler<PaintEventArgs>, PaintEventArgs> = this.Paint

        // Properties
        member this.Bounds with get() : Rectangle = this.Bounds
        member this.FlowDirection with get() : FlowDirection = this.FlowDirection
        member this.Handle with get() : nativeint = this.Handle
        member this.IsActive with get() : bool = this.IsActive
        member this.IsVisible with get() : bool = this.IsVisible
        member this.ParentThread with get() : Thread = this.ParentThread
        member this.ReadingDirection with get() : ReadingDirection = this.ReadingDirection
        member this.Title with get() : string = this.Title
        member this.WindowManager with get() : IWindowManager = this.WindowManager

        // Methods
        member this.Activate() = this.Activate()
        member this.Close() = this.Close()
        member this.Hide() = this.Hide()
        member this.Show() = this.Show()
