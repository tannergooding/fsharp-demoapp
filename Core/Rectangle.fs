namespace DemoApplication

[<Struct>]
type Rectangle =
    // Fields
    val public Location:Point2D
    val public Size:Size2D

    // Constructors
    new (location:Point2D, size:Size2D) = { Location = location; Size = size }
    new (x:float32, y:float32, width:float32, height:float32) = { Location = new Point2D(x, y); Size = new Size2D(width, height) }

    // Properties
    member public this.Height with get() : float32 = this.Size.Height

    member public this.Width with get() : float32 = this.Size.Width

    member public this.X with get() : float32 = this.Location.X

    member public this.Y with get() : float32 = this.Location.Y
