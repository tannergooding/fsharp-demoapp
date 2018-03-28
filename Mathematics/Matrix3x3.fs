namespace DemoApplication.Mathematics

open System.Runtime.Intrinsics
open System.Runtime.Intrinsics.X86

[<Struct>]
type Matrix3x3 =
    // Fields
    val public X:Vector3
    val public Y:Vector3
    val public Z:Vector3

    // Constructors
    public new (x:Vector3, y:Vector3, z:Vector3) =
        { X = x; Y = y; Z = z }

    // Operators
    static member public (*) (left:Matrix3x3, right:Matrix3x3) : Matrix3x3 =
        let x:Vector3 = left.X * right
        let y:Vector3 = left.Y * right
        let z:Vector3 = left.Z * right

        new Matrix3x3(x, y, z)

    static member public (*) (left:Vector3, right:Matrix3x3) : Vector3 =
#if HWIntrinsics
        let mutable xx:Vector128<float32> = IntrinsicMath.Permute(left.Value, IntrinsicMath.Shuffle(0uy, 0uy, 0uy, 0uy))
        let mutable xy:Vector128<float32> = IntrinsicMath.Permute(left.Value, IntrinsicMath.Shuffle(1uy, 1uy, 1uy, 1uy))
        let mutable xz:Vector128<float32> = IntrinsicMath.Permute(left.Value, IntrinsicMath.Shuffle(2uy, 2uy, 2uy, 2uy))

        xx <- Sse.Multiply(xx, right.X.Value)
        xy <- Sse.Multiply(xy, right.Y.Value)
        xz <- Sse.Multiply(xz, right.Z.Value)

        let mutable temp:Vector128<float32> = Sse.Add(xx, xy)
        temp <- Sse.Add(temp, xz)
        new Vector3(temp)
#else
        new Vector3((left.X * right.X.X) + (left.Y * right.Y.X) + (left.Z * right.Z.X),
                    (left.X * right.X.Y) + (left.Y * right.Y.Y) + (left.Z * right.Z.Y),
                    (left.X * right.X.Z) + (left.Y * right.Y.Z) + (left.Z * right.Z.Z))
#endif
