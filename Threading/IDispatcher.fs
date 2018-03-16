namespace DemoApplication.Threading

open DemoApplication
open System
open System.Runtime.InteropServices
open System.Threading;

type IDispatcher =
    // Events
    [<CLIEvent>]
    abstract member ExitRequested:IEvent<EventHandler, EventArgs>

    // Properties
    abstract member DispatchManager:IDispatchManager with get
    abstract member ParentThread:Thread with get

    // Methods
    abstract member DispatchPending:unit -> unit
and [<AllowNullLiteral>] IDispatchManager =
    // Properties
    abstract member CurrentTimestamp:Timestamp with get
    abstract member DispatcherForCurrentThread:IDispatcher with get

    // Methods
    abstract member GetDispatcher : thread:Thread -> IDispatcher
    abstract member TryGetDispatcher : thread:Thread * [<Out>] dispatcher:IDispatcher ref -> bool
