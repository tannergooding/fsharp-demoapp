namespace DemoApplication

[<Struct>]
type Timestamp =
    // Fields
    val private ticks:int64

    // Constructors
    new (ticks:int64) = { ticks = ticks }

    // Properties
    member public this.Ticks with get() : int64 = this.ticks

    // Operators
    static member (+) (left:Timestamp, right:Timestamp) = Timestamp(left.ticks + right.ticks)
    static member (-) (left:Timestamp, right:Timestamp) = Timestamp(left.ticks - right.ticks)
