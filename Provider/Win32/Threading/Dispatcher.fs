namespace DemoApplication.Provider.Win32.Threading

open DemoApplication
open DemoApplication.Interop
open DemoApplication.Threading
open DemoApplication.Utilities
open System
open System.Collections.Generic
open System.Runtime.InteropServices
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
and DispatchManager public () =
    // Fields
    let tickFrequency:float =
        let mutable frequency:LARGE_INTEGER = Unchecked.defaultof<LARGE_INTEGER>

        if Kernel32.QueryPerformanceFrequency(&frequency) = Windows.FALSE then
            ExceptionUtilities.ThrowExternalExceptionForLastError "QueryPerformanceFrequency"

        float TimeSpan.TicksPerSecond / float frequency

    let dispatchers:Dictionary<Thread, IDispatcher> = Dictionary<Thread, IDispatcher>()

    // Properties
    member this.CurrentTimestamp
        with get() : Timestamp =
            let mutable performanceCount:LARGE_INTEGER = Unchecked.defaultof<LARGE_INTEGER>

            if Kernel32.QueryPerformanceCounter(&performanceCount) = Windows.FALSE then
                ExceptionUtilities.ThrowExternalExceptionForLastError "QueryPerformanceCounter"

            let ticks = int64 (float performanceCount * tickFrequency)
            new Timestamp (ticks)

    member this.DispatcherForCurrentThread with get() : IDispatcher = this.GetDispatcher Thread.CurrentThread

    // Methods
    member this.GetDispatcher(thread:Thread) : IDispatcher =
        match this.TryGetDispatcher(thread) with
        | (true, dispatcher) -> dispatcher
        | (false, _) -> new Dispatcher(this, thread) :> IDispatcher

    member this.TryGetDispatcher(thread:Thread, [<Out>] dispatcher:byref<IDispatcher>) : bool =
        ExceptionUtilities.ThrowIfNull("thread", thread)
        dispatchers.TryGetValue(thread, ref dispatcher)

    // DemoApplication.Threading.IDispatchManager
    interface IDispatchManager with
        // Properties
        member this.CurrentTimestamp with get() : Timestamp = this.CurrentTimestamp
        member this.DispatcherForCurrentThread with get() : IDispatcher = this.DispatcherForCurrentThread

        // Methods
        member this.GetDispatcher(thread:Thread) : IDispatcher = this.GetDispatcher(thread)
        member this.TryGetDispatcher(thread:Thread, [<Out>] dispatcher:byref<IDispatcher>) : bool = this.TryGetDispatcher(thread, ref dispatcher)