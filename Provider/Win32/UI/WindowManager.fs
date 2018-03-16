namespace DemoApplication.Provider.Win32.UI

open DemoApplication.Interop
open DemoApplication.Threading
open DemoApplication.UI
open DemoApplication.Utilities
open Microsoft.FSharp.NativeInterop
open System
open System.Collections.Generic
open System.Runtime.InteropServices

type WindowManager public (dispatchManager:IDispatchManager) as this =
    // Static Fields
    static let EntryPointModule:HMODULE = Kernel32.GetModuleHandle(NativePtr.ofNativeInt 0n)

    // Fields
    let ForwardWndProc:WNDPROC = new WNDPROC(this.ForwardWindowMessage)

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

    let dispatchManager:IDispatchManager = dispatchManager
    let windows:Dictionary<HWND, IWindow> = new Dictionary<HWND, IWindow>()

    // Properties
    member this.Handle with get() : nativeint = nativeint classAtom.Value
    member this.DispatchManager with get() : IDispatchManager = dispatchManager
    member this.Windows with get() : IEnumerable<IWindow> = windows.Values :> IEnumerable<IWindow>

    // Static Methods
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

    member internal this.DisposeClassAtom : unit =
        if classAtom.IsValueCreated && (User32.UnregisterClass(NativePtr.ofNativeInt this.Handle, EntryPointModule) = Windows.FALSE) then
            ExceptionUtilities.ThrowExternalExceptionForLastError "UnregisterClass"

    member internal this.DisposeWindows : unit =
        for window in windows.Values do
            (window :?> Window).Dispose()
        windows.Clear()

    member internal this.ForwardWindowMessage (hWnd:HWND) (msg:UINT) (wParam:WPARAM) (lParam:LPARAM) : LRESULT =
        let mutable window:IWindow = Unchecked.defaultof<IWindow>

        if windows.TryGetValue(hWnd, &window) then
            if msg = User32.WM_DESTROY then
                windows.Remove(hWnd) |> ignore

                if windows.Count = 0 then
                    User32.PostQuitMessage(0)


            (window :?> Window).ProcessWindowMessage(msg, wParam, lParam)
        else
            User32.DefWindowProc(hWnd, msg, wParam, lParam)

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
        member this.Windows with get() : IEnumerable<IWindow> = this.Windows

        // Methods
        member this.CreateWindow() : IWindow = this.CreateWindow()
