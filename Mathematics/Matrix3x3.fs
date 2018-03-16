namespace DemoApplication.Mathematics

[<Struct>]
type Matrix3x3 =
    // Fields
    val public x:Vector3
    val public y:Vector3
    val public z:Vector3

    // Constructors
    new (x, y, z) =
        { x = x; y = y; z = z }

    // Operators
    static member (*) (left:Matrix3x3, right:Matrix3x3) =
        let x:Vector3 = new Vector3((left.x.x * right.x.x) + (left.x.y * right.y.x) + (left.x.z * right.z.x),
                                    (left.x.x * right.x.y) + (left.x.y * right.y.y) + (left.x.z * right.z.y),
                                    (left.x.x * right.x.z) + (left.x.y * right.y.z) + (left.x.z * right.z.z))

        let y:Vector3 = new Vector3((left.y.x * right.x.x) + (left.y.y * right.y.x) + (left.y.z * right.z.x),
                                    (left.y.x * right.x.y) + (left.y.y * right.y.y) + (left.y.z * right.z.y),
                                    (left.y.x * right.x.z) + (left.y.y * right.y.z) + (left.y.z * right.z.z))

        let z:Vector3 = new Vector3((left.z.x * right.x.x) + (left.z.y * right.y.x) + (left.z.z * right.z.x),
                                    (left.z.x * right.x.y) + (left.z.y * right.y.y) + (left.z.z * right.z.y),
                                    (left.z.x * right.x.z) + (left.z.y * right.y.z) + (left.z.z * right.z.z))

        new Matrix3x3(x, y, z)

    static member (*) (left:Vector3, right:Matrix3x3) =
        new Vector3((left.x * right.x.x) + (left.y * right.y.x) + (left.z * right.z.x),
                    (left.x * right.x.y) + (left.y * right.y.y) + (left.z * right.z.y),
                    (left.x * right.x.z) + (left.y * right.y.z) + (left.z * right.z.z))
