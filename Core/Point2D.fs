namespace DemoApplication

[<Struct>]
type Point2D =
    // Fields
    val public X:float32
    val public Y:float32

    // Constructors
    new (x:float32, y:float32) = { X = x; Y = y; }
