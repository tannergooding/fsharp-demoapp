namespace DemoApplication

[<Struct>]
type Size2D =
    // Fields
    val public Width:float32
    val public Height:float32

    // Constructors
    new (width:float32, height:float32) = { Width = width; Height = height }
