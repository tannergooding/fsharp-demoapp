namespace DemoApplication.UI

open DemoApplication
open System
open System.Collections.Generic
open System.Threading

type IWindow =
    // Events
    [<CLIEvent>]
    abstract member Paint:IEvent<EventHandler<PaintEventArgs>, PaintEventArgs>

    // Properties
    abstract member Bounds:Rectangle with get
    abstract member FlowDirection:FlowDirection with get
    abstract member Handle:nativeint with get
    abstract member IsActive:bool with get
    abstract member IsVisible:bool with get
    abstract member ParentThread:Thread with get
    abstract member ReadingDirection:ReadingDirection with get
    abstract member Title:string with get
    abstract member WindowManager:IWindowManager with get

    // Methods
    abstract member Activate:unit -> unit
    abstract member Close:unit -> unit
    abstract member Hide:unit -> unit
    abstract member Show:unit -> unit
and IWindowManager =
    // Properties
    abstract member Handle:nativeint with get
    abstract member Windows:IEnumerable<IWindow> with get

    // Methods
    abstract member CreateWindow:unit -> IWindow
