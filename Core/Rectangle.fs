namespace DemoApplication

[<Struct>]
type Rectangle =
    // Fields
    val mutable public Location:Point2D
    val mutable public Size:Size2D

    // Constructors
    new (location:Point2D, size:Size2D) = { Location = location; Size = size }
    new (x:float32, y:float32, width:float32, height:float32) = { Location = new Point2D(x, y); Size = new Size2D(width, height) }
