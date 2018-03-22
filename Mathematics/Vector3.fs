namespace DemoApplication.Mathematics

[<Struct>]
type Vector3 =
    // Fields
    val mutable public x:float32
    val mutable public y:float32
    val mutable public z:float32

    // Constructors
    new (x, y, z) =
        { x = x; y = y; z = z }

    // Static Properties
    static member UnitX with get() : Vector3 = new Vector3(1.0f, 0.0f, 0.0f)
    static member UnitY with get() : Vector3 = new Vector3(0.0f, 1.0f, 0.0f)
    static member UnitZ with get() : Vector3 = new Vector3(0.0f, 0.0f, 1.0f)

    // Properties
    member this.Length with get() : float32 = sqrt this.LengthSquared

    member this.LengthSquared with get() : float32 = Vector3.DotProduct(this, this)

    // Operators
    static member (+) (left:Vector3, right:Vector3) =
        new Vector3(left.x + right.x,
                    left.y + right.y,
                    left.z + right.z)

    static member (-) (left:Vector3, right:Vector3) =
        new Vector3(left.x - right.x,
                    left.y - right.y,
                    left.z - right.z)

    static member (*) (left:Vector3, right:float32) =
        new Vector3(left.x * right,
                    left.y * right,
                    left.z * right)

    static member (*) (left:Vector3, right:Vector3) =
        new Vector3(left.x * right.x,
                    left.y * right.y,
                    left.z * right.z)

    static member (/) (left:Vector3, right:float32) =
        new Vector3(left.x / right,
                    left.y / right,
                    left.z / right)

    static member (/) (left:Vector3, right:Vector3) =
        new Vector3(left.x / right.x,
                    left.y / right.y,
                    left.z / right.z)

    // Static Methods
    static member DotProduct(left:Vector3, right:Vector3) : float32 =
        (left.x * right.x) +
        (left.y * right.y) +
        (left.z * right.z)

    // Methods
    member this.Normalize() : Vector3 =
        this / this.Length
