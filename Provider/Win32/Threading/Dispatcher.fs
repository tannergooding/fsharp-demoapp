namespace DemoApplication.Provider.Win32.Threading

open DemoApplication.Interop
open DemoApplication.Threading
open DemoApplication.Utilities
open System
open System.Threading

type Dispatcher internal (dispatchManager:IDispatchManager, parentThread:Thread) =
    // Fields
    let exitRequested:Event<EventHandler, EventArgs> = Event<EventHandler, EventArgs>()

    let dispatchManager:IDispatchManager =
        ExceptionUtilities.ThrowIfNull("dispatchManager", dispatchManager)
        dispatchManager

    let parentThread:Thread =
        ExceptionUtilities.ThrowIfNull("parentThread", parentThread)
        parentThread

    // Events
    [<CLIEvent>]
    member public this.ExitRequested:IEvent<EventHandler, EventArgs> = exitRequested.Publish

    // Properties
    member public this.DispatchManager with get() : IDispatchManager = dispatchManager
    member public this.ParentThread with get() : Thread = parentThread

    // Methods
    member public this.DispatchPending() =
        ExceptionUtilities.ThrowIfNotThread parentThread

        let mutable msg:MSG = Unchecked.defaultof<MSG>

        while User32.PeekMessage(&msg, HWND Windows.NULL, User32.WM_NULL, User32.WM_NULL, User32.PM_REMOVE) <> Windows.FALSE do
            if msg.message <> User32.WM_QUIT then
                User32.DispatchMessage(&msg) |> ignore
            else
                this.OnExitRequested()

    member internal this.OnExitRequested() =
        exitRequested.Trigger(this, EventArgs.Empty)

    // DemoApplication.Threading.IDispatcher
    interface IDispatcher with
        // Events
        [<CLIEvent>]
        member this.ExitRequested:IEvent<EventHandler, EventArgs> = this.ExitRequested

        // Properties
        member this.DispatchManager with get() : IDispatchManager = this.DispatchManager
        member this.ParentThread with get() : Thread = this.ParentThread

        // Methods
        member this.DispatchPending() = this.DispatchPending()
