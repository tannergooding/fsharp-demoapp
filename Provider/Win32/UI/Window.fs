namespace DemoApplication.Provider.Win32.UI

open DemoApplication
open DemoApplication.Interop
open DemoApplication.Provider.Win32.Graphics
open DemoApplication.Threading
open DemoApplication.UI
open DemoApplication.Utilities
open Microsoft.FSharp.NativeInterop
open System
open System.Collections.Generic
open System.Threading
open System.Runtime.InteropServices

type Window internal (windowManager:IWindowManager, entryPointModule:HMODULE) =
    // Fields
    let paint:Event<EventHandler<PaintEventArgs>, PaintEventArgs> = Event<EventHandler<PaintEventArgs>, PaintEventArgs>()
    let windowManager:IWindowManager = windowManager
    let mutable title:string = typeof<Window>.FullName
    let parentThread:Thread = Thread.CurrentThread

    let mutable bounds:Rectangle = new Rectangle(Single.NaN, Single.NaN, Single.NaN, Single.NaN)
    let flowDirection:FlowDirection = FlowDirection.TopToBottom
    let readingDirection:ReadingDirection = ReadingDirection.LeftToRight
    let mutable windowState:WindowState = WindowState.Restored
    let mutable isActive:bool = false
    let mutable isEnabled:bool = true
    let mutable isVisible:bool = false

    let hdd:Lazy<HDRAWDIB> = lazy Msvfw32.DrawDibOpen()
    let handle:Lazy<HWND> = lazy (
        use windowName = fixed title

        let windowStyle = User32.WS_OVERLAPPEDWINDOW |||
                          (if windowState = WindowState.Minimized then User32.WS_MAXIMIZE else if windowState = WindowState.Maximized then User32.WS_MINIMIZE else 0u) |||
                          (if not isEnabled then User32.WS_DISABLED else 0u) |||
                          (if isVisible then User32.WS_VISIBLE else 0u)

        let windowStyleEx = User32.WS_EX_OVERLAPPEDWINDOW |||
                            (if flowDirection = FlowDirection.RightToLeft then User32.WS_EX_LAYOUTRTL else 0u) |||
                            (if readingDirection = ReadingDirection.RightToLeft then (User32.WS_EX_RIGHT ||| User32.WS_EX_RTLREADING ||| User32.WS_EX_LEFTSCROLLBAR) else 0u)

        let hWnd:HWND = User32.CreateWindowEx(windowStyleEx,
                                              NativePtr.ofNativeInt (windowManager.Handle),
                                              windowName,
                                              windowStyle,
                                              (if Single.IsNaN(bounds.Location.X) then User32.CW_USEDEFAULT else int32 bounds.Location.X),
                                              (if Single.IsNaN(bounds.Location.Y) then User32.CW_USEDEFAULT else int32 bounds.Location.Y),
                                              (if Single.IsNaN(bounds.Size.Width) then User32.CW_USEDEFAULT else int32 bounds.Size.Width),
                                              (if Single.IsNaN(bounds.Size.Height) then User32.CW_USEDEFAULT else int32 bounds.Size.Height),
                                              HWND Windows.NULL,
                                              HMENU Windows.NULL,
                                              entryPointModule,
                                              GCHandle.ToIntPtr((windowManager :?> WindowManager).NativeHandle))

        if hWnd = HWND Windows.NULL then
            ExceptionUtilities.ThrowExternalExceptionForLastError "CreateWindowEx"

        hWnd
    )

    // Events
    [<CLIEvent>]
    member public this.Paint:IEvent<EventHandler<PaintEventArgs>, PaintEventArgs> = paint.Publish

    // Properties
    member this.Bounds with get() : Rectangle = bounds
    member this.FlowDirection with get() : FlowDirection = flowDirection
    member this.Handle with get() : nativeint = handle.Value
    member this.IsActive with get() : bool = isActive
    member this.IsEnabled with get() : bool = isEnabled
    member this.IsVisible with get() : bool = isVisible
    member this.ParentThread with get() : Thread = parentThread
    member this.ReadingDirection with get() : ReadingDirection = readingDirection
    member this.Title with get() : string = title
    member this.WindowManager with get() : IWindowManager = windowManager
    member this.WindowState with get() : WindowState = windowState

    // Static Methods
    static member internal GetWindowBounds(handle:HWND) : Rectangle =
        let mutable rect:RECT = Unchecked.defaultof<RECT>

        if User32.GetWindowRect(handle, &rect) = Windows.FALSE then
            ExceptionUtilities.ThrowExternalExceptionForLastError "GetWindowRect"

        new Rectangle(float32 rect.left, float32 rect.top, float32 (rect.right - rect.left), float32 (rect.bottom - rect.top))

    static member internal IsWindowActive(handle:HWND) : bool =
        User32.GetActiveWindow() = handle

    // Methods
    member public this.Activate() : bool =
        isActive || (User32.SetForegroundWindow(this.Handle) <> Windows.FALSE)

    member public this.Close() : unit =
        if handle.IsValueCreated then
            User32.SendMessage(this.Handle, User32.WM_CLOSE, 0un, 0n) |> ignore

    member public this.Disable() : unit =
        if isEnabled then
            User32.EnableWindow(this.Handle, Windows.FALSE) |> ignore

    member public this.Enable() : unit =
        if not isEnabled then
            User32.EnableWindow(this.Handle, Windows.TRUE) |> ignore

    member public this.Dispose() : unit =
        this.Dispose true
        GC.SuppressFinalize(this)

    member public this.Hide() : unit =
        if isVisible then
            User32.ShowWindow(this.Handle, User32.SW_HIDE) |> ignore

    member public this.Maximize() : unit =
        if windowState <> WindowState.Maximized then
            User32.ShowWindow(this.Handle, User32.SW_MAXIMIZE) |> ignore

    member public this.Minimize() : unit =
        if windowState <> WindowState.Minimized then
            User32.ShowWindow(this.Handle, User32.SW_MINIMIZE) |> ignore

    member public this.Redraw() : unit =
        ExceptionUtilities.ThrowIfNotThread parentThread

        let hdc = User32.GetDC(this.Handle)

        let drawingContext = new DrawingContext(hdd.Value, hdc)
        let eventArgs = new PaintEventArgs(drawingContext)
        paint.Trigger(this, eventArgs)

        User32.ReleaseDC(this.Handle, hdc) |> ignore

    member public this.Restore() : unit =
        if windowState <> WindowState.Restored then
            User32.ShowWindow(this.Handle, User32.SW_RESTORE) |> ignore

    member public this.Show() : unit =
        if not isVisible then
            User32.ShowWindow(this.Handle, User32.SW_SHOW) |> ignore

    member internal this.Dispose (isDisposing:bool) : unit =
        if Thread.CurrentThread <> parentThread then
            this.Close()
        else
            this.DisposeDrawDibHandle()
            this.DisposeWindowHandle()

    member internal this.DisposeDrawDibHandle() : unit =
        if hdd.IsValueCreated then
            Msvfw32.DrawDibClose(hdd.Value) |> ignore

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

    member internal this.HandleWmEnable(wParam:WPARAM) : LRESULT =
        isEnabled <- (wParam <> unativeint Windows.FALSE)
        0n

    member internal this.HandleWmMove(lParam:LPARAM) : LRESULT =
        bounds <- new Rectangle(new Point2D(float32(Windows.LOWORD(unativeint lParam)), float32(Windows.HIWORD(unativeint lParam))), bounds.Size)
        0n

    member internal this.HandleWmSetText(wParam:WPARAM, lParam:LPARAM) : LRESULT =
        let result = User32.DefWindowProc(this.Handle, User32.WM_SETTEXT, wParam, lParam)

        if result <> nativeint Windows.FALSE then
            title <- Marshal.PtrToStringUni(lParam)

        result

    member internal this.HandleWmShowWindow(wParam:WPARAM) : LRESULT =
        isVisible <- Windows.LOWORD(wParam) <> uint16 Windows.FALSE
        0n

    member internal this.HandleWmSize(wParam:WPARAM, lParam:LPARAM) : LRESULT =
        windowState <- LanguagePrimitives.EnumOfValue (int32 wParam)
        bounds <- new Rectangle(bounds.Location, new Size2D(float32(Windows.LOWORD(unativeint lParam)), float32(Windows.HIWORD(unativeint lParam))))
        0n

    member internal this.ProcessWindowMessage(msg:UINT, wParam:WPARAM, lParam:LPARAM) : LRESULT =
        ExceptionUtilities.ThrowIfNotThread parentThread

        match msg with
        | User32.WM_DESTROY -> this.HandleWmDestroy()
        | User32.WM_MOVE -> this.HandleWmMove lParam
        | User32.WM_SIZE -> this.HandleWmSize(wParam, lParam)
        | User32.WM_ACTIVATE -> this.HandleWmActivate wParam
        | User32.WM_ENABLE -> this.HandleWmEnable wParam
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
        member this.IsEnabled with get() : bool = this.IsEnabled
        member this.IsVisible with get() : bool = this.IsVisible
        member this.ParentThread with get() : Thread = this.ParentThread
        member this.ReadingDirection with get() : ReadingDirection = this.ReadingDirection
        member this.Title with get() : string = this.Title
        member this.WindowManager with get() : IWindowManager = this.WindowManager
        member this.WindowState with get() : WindowState = this.WindowState

        // Methods
        member this.Activate() : bool = this.Activate()
        member this.Close() = this.Close()
        member this.Disable() = this.Disable()
        member this.Enable() = this.Enable()
        member this.Hide() = this.Hide()
        member this.Maximize() = this.Maximize()
        member this.Minimize() = this.Minimize()
        member this.Restore() = this.Restore()
        member this.Show() = this.Show()
and WindowManager public (dispatchManager:IDispatchManager) as this =
    // Static Fields
    static let EntryPointModule:HMODULE = Kernel32.GetModuleHandle(NativePtr.ofNativeInt 0n)
    static let ForwardWndProc:WNDPROC = new WNDPROC(WindowManager.ForwardWindowMessage)

    // Fields
    let classAtom:Lazy<ATOM> = lazy (
        use className = fixed String.Format("{0}.{1:X16}.{2:X8}", typeof<WindowManager>.FullName, EntryPointModule, this.GetHashCode())

        let mutable windowClass:WNDCLASSEX = Unchecked.defaultof<WNDCLASSEX>
        windowClass.cbSize <- uint32 sizeof<WNDCLASSEX>
        windowClass.style <- (User32.CS_VREDRAW ||| User32.CS_HREDRAW ||| User32.CS_DBLCLKS ||| User32.CS_OWNDC)
        windowClass.lpfnWndProc <- Marshal.GetFunctionPointerForDelegate(ForwardWndProc)
        windowClass.hInstance <- EntryPointModule
        windowClass.hCursor <- WindowManager.GetDesktopCursor
        windowClass.hbrBackground <- nativeint (User32.COLOR_WINDOW + 1)
        windowClass.lpszClassName <- className

        let classAtom:ATOM = User32.RegisterClassEx(&windowClass)

        if classAtom = uint16 Windows.NULL then
            ExceptionUtilities.ThrowExternalExceptionForLastError "RegisterClassEx"

        classAtom
    )

    let nativeHandle:Lazy<GCHandle> = lazy GCHandle.Alloc(this, GCHandleType.Normal)
    let dispatchManager:IDispatchManager = dispatchManager
    let windows:Dictionary<HWND, IWindow> = new Dictionary<HWND, IWindow>()

    // Properties
    member this.Handle with get() : nativeint = nativeint classAtom.Value
    member this.DispatchManager with get() : IDispatchManager = dispatchManager
    member this.NativeHandle with get() : GCHandle = nativeHandle.Value
    member this.Windows with get() : Dictionary<HWND, IWindow> = windows

    // Static Methods
    static member internal ForwardWindowMessage (hWnd:HWND) (msg:UINT) (wParam:WPARAM) (lParam:LPARAM) : LRESULT =
        let userData:nativeint =
            if msg = User32.WM_CREATE then
                let createStruct:byref<CREATESTRUCT> = NativePtr.toByRef(NativePtr.ofNativeInt lParam)
                let lpCreateParams = createStruct.lpCreateParams

                User32.SetWindowLongPtr(hWnd, User32.GWLP_USERDATA, lpCreateParams) |> ignore
                lpCreateParams
            else
                User32.GetWindowLongPtr(hWnd, User32.GWLP_USERDATA)

        if userData <> 0n then
            let windowManager = GCHandle.FromIntPtr(userData).Target :?> WindowManager

            match windowManager.Windows.TryGetValue(hWnd) with
            | (true, window) -> if msg = User32.WM_DESTROY then
                                    windowManager.Windows.Remove(hWnd) |> ignore

                                    if windowManager.Windows.Count = 0 then
                                        User32.PostQuitMessage(0)

                                (window :?> Window).ProcessWindowMessage(msg, wParam, lParam)
            | (false, _) -> User32.DefWindowProc(hWnd, msg, wParam, lParam)
        else
            User32.DefWindowProc(hWnd, msg, wParam, lParam)

    static member internal GetDesktopCursor : HCURSOR =
        let desktopHwnd:HWND = User32.GetDesktopWindow()
        let desktopClassName:nativeptr<char> = NativePtr.stackalloc(256)

        if User32.GetClassName(desktopHwnd, desktopClassName, 256) = 0 then
            ExceptionUtilities.ThrowExternalExceptionForLastError "GetClassName"

        let mutable desktopWindowClass:WNDCLASSEX = Unchecked.defaultof<WNDCLASSEX>

        if User32.GetClassInfoEx(HINSTANCE Windows.NULL, desktopClassName, &desktopWindowClass) = Windows.FALSE then
            ExceptionUtilities.ThrowExternalExceptionForLastError "GetClassInfoEx"

        desktopWindowClass.hCursor

    // Methods
    member public this.CreateWindow() : IWindow =
        let window = new Window(this, EntryPointModule) :> IWindow
        windows.Add(window.Handle, window)
        window

    member public this.Dispose() : unit =
        this.Dispose true
        GC.SuppressFinalize(this)

    member internal this.Dispose (isDisposing:bool) : unit =
        this.DisposeWindows
        this.DisposeClassAtom
        this.DisposeNativeHandle

    member internal this.DisposeClassAtom : unit =
        if classAtom.IsValueCreated && (User32.UnregisterClass(NativePtr.ofNativeInt this.Handle, EntryPointModule) = Windows.FALSE) then
            ExceptionUtilities.ThrowExternalExceptionForLastError "UnregisterClass"

    member internal this.DisposeNativeHandle : unit =
        if nativeHandle.IsValueCreated then
            nativeHandle.Value.Free()

    member internal this.DisposeWindows : unit =
        for window in windows.Values do
            (window :?> Window).Dispose()
        windows.Clear()

    // System.IDisposable
    interface IDisposable with
        // Methods
        member this.Dispose() : unit = this.Dispose()

    // System.Object
    override this.Finalize() =
        this.Dispose false

    // DemoApplication.UI.IWindowManager
    interface IWindowManager with
        // Properties
        member this.Handle with get() : nativeint = this.Handle
        member this.Windows with get() : IEnumerable<IWindow> = windows.Values :> IEnumerable<IWindow>

        // Methods
        member this.CreateWindow() : IWindow = this.CreateWindow()
