namespace DemoApplication.Provider.Win32.Threading

open DemoApplication
open DemoApplication.Interop
open DemoApplication.Threading
open DemoApplication.Utilities
open System
open System.Collections.Generic
open System.Runtime.InteropServices
open System.Threading

type DispatchManager public () =
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
        let mutable dispatcher:IDispatcher = Unchecked.defaultof<IDispatcher>

        if not (this.TryGetDispatcher(thread, ref dispatcher)) then
            dispatcher <- new Dispatcher(this, thread)

        dispatcher

    member this.TryGetDispatcher(thread:Thread, [<Out>] dispatcher:IDispatcher ref) : bool =
        ExceptionUtilities.ThrowIfNull("thread", thread)

        dispatchers.TryGetValue(thread, dispatcher)

    // DemoApplication.Threading.IDispatchManager
    interface IDispatchManager with
        // Properties
        member this.CurrentTimestamp with get() : Timestamp = this.CurrentTimestamp
        member this.DispatcherForCurrentThread with get() : IDispatcher = this.DispatcherForCurrentThread

        // Methods
        member this.GetDispatcher(thread:Thread) : IDispatcher = this.GetDispatcher(thread)
        member this.TryGetDispatcher(thread:Thread, [<Out>] dispatcher:IDispatcher ref) : bool = this.TryGetDispatcher(thread, dispatcher)
