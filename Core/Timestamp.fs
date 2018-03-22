namespace DemoApplication

open System

[<Struct>]
type Timestamp =
    // Fields
    val public Ticks:int64

    // Constructors
    new (ticks:int64) = { Ticks = ticks }

    // Operators
    static member (+) (left:Timestamp, right:Timestamp) : TimeSpan = TimeSpan(left.Ticks + right.Ticks)
    static member (-) (left:Timestamp, right:Timestamp) : TimeSpan = TimeSpan(left.Ticks - right.Ticks)
