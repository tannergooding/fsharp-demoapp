namespace DemoApplication.Mathematics

open System
open System.Runtime.Intrinsics
open System.Runtime.Intrinsics.X86

[<Struct>]
type Vector3 =
    // Fields
#if HWIntrinsics
    val public Value:Vector128<float32>
#else
    val public X:float32
    val public Y:float32
    val public Z:float32
#endif

    // Constructors
    public new (value:float32) =
#if HWIntrinsics
        { Value = Sse.SetAllVector128(value) }
#else
        { X = value; Y = value; Z = value }
#endif

    public new (x:float32, y:float32, z:float32) =
#if HWIntrinsics
        { Value = Sse.SetVector128(0.0f, z, y, x) }
#else
        { X = x; Y = y; Z = z }
#endif

#if HWIntrinsics
    public new (value:Vector128<float32>) =
        { Value = value }
#endif

    // Static Properties
    static member public UnitX with get() : Vector3 = new Vector3(1.0f, 0.0f, 0.0f)
    static member public UnitY with get() : Vector3 = new Vector3(0.0f, 1.0f, 0.0f)
    static member public UnitZ with get() : Vector3 = new Vector3(0.0f, 0.0f, 1.0f)

    // Properties
#if HWIntrinsics
    member public this.Length with get() : float32 = let mutable temp:Vector128<float32> = IntrinsicMath.DotVector3(this.Value, this.Value)
                                                     temp <- Sse.Sqrt(temp)
                                                     Sse.ConvertToSingle(temp)

    member public this.LengthSquared with get() : float32 = let temp:Vector128<float32> = IntrinsicMath.DotVector3(this.Value, this.Value)
                                                            Sse.ConvertToSingle(temp)

    member public this.X with get() : float32 = Sse.ConvertToSingle(this.Value)

    member public this.Y with get() : float32 = if Sse41.IsSupported then
                                                    Sse41.Extract(this.Value, 1uy)
                                                else
                                                    let temp = IntrinsicMath.Permute(this.Value, IntrinsicMath.Shuffle(1uy, 1uy, 1uy, 1uy))
                                                    Sse.ConvertToSingle(temp)

    member public this.Z with get() : float32 = if Sse41.IsSupported then
                                                    Sse41.Extract(this.Value, 2uy)
                                                else
                                                    let temp = IntrinsicMath.Permute(this.Value, IntrinsicMath.Shuffle(2uy, 2uy, 2uy, 2uy))
                                                    Sse.ConvertToSingle(temp)
#else
    member public this.Length with get() : float32 = MathF.Sqrt(this.LengthSquared)

    member public this.LengthSquared with get() : float32 = Vector3.DotProduct(this, this)
#endif

    // Operators
    static member (+) (left:Vector3, right:Vector3) =
#if HWIntrinsics
        let result:Vector128<float32> = Sse.Add(left.Value, right.Value)
        new Vector3(result)
#else
        new Vector3(left.X + right.X,
                    left.Y + right.Y,
                    left.Z + right.Z)
#endif

    static member (-) (left:Vector3, right:Vector3) =
#if HWIntrinsics
        let result:Vector128<float32> = Sse.Subtract(left.Value, right.Value)
        new Vector3(result)
#else
        new Vector3(left.X - right.X,
                    left.Y - right.Y,
                    left.Z - right.Z)
#endif

    static member (*) (left:Vector3, right:float32) =
#if HWIntrinsics
        left * new Vector3(right)
#else
        new Vector3(left.X * right,
                    left.Y * right,
                    left.Z * right)
#endif

    static member (*) (left:Vector3, right:Vector3) =
#if HWIntrinsics
        let result:Vector128<float32> = Sse.Multiply(left.Value, right.Value)
        new Vector3(result)
#else
        new Vector3(left.X * right.X,
                    left.Y * right.Y,
                    left.Z * right.Z)
#endif

    static member (/) (left:Vector3, right:float32) =
#if HWIntrinsics
        left / new Vector3(right)
#else
        new Vector3(left.X / right,
                    left.Y / right,
                    left.Z / right)
#endif

    static member (/) (left:Vector3, right:Vector3) =
#if HWIntrinsics
        let result:Vector128<float32> = Sse.Divide(left.Value, right.Value)
        new Vector3(result)
#else
        new Vector3(left.X / right.X,
                    left.Y / right.Y,
                    left.Z / right.Z)
#endif

    // Static Methods
    static member DotProduct(left:Vector3, right:Vector3) : float32 =
#if HWIntrinsics
        let result:Vector128<float32> = IntrinsicMath.DotVector3(left.Value, right.Value)
        Sse.ConvertToSingle(result)
#else
        (left.X * right.X) +
        (left.Y * right.Y) +
        (left.Z * right.Z)
#endif

    // Methods
    member this.Normalize() : Vector3 =
#if HWIntrinsics
        let mutable length:Vector128<float32> = IntrinsicMath.DotVector3(this.Value, this.Value)
        length <- Sse.Sqrt(length)

        let result:Vector128<float32> = Sse.Divide(this.Value, length)
        new Vector3(result)
#else
        this / this.Length
#endif

    member this.WithX(value:float32) : Vector3 =
#if HWIntrinsics
        let mutable result:Vector128<float32> = Sse.SetScalarVector128(value)
        result <- Sse.MoveScalar(this.Value, result)
        new Vector3(result)
#else
        new Vector3(value, this.Y, this.Z)
#endif

    member this.WithY(value:float32) : Vector3 =
#if HWIntrinsics
        if Sse41.IsSupported then
            let result:Vector128<float32> = Sse41.Insert(this.Value, value, 0x10uy)
            new Vector3(result)
        else
            let mutable result:Vector128<float32> = IntrinsicMath.Permute(this.Value, IntrinsicMath.Shuffle(3uy, 2uy, 0uy, 1uy))
            let temp:Vector128<float32> = Sse.SetScalarVector128(value)
            result <- Sse.MoveScalar(result, temp)
            result <- IntrinsicMath.Permute(result, IntrinsicMath.Shuffle(3uy, 2uy, 0uy, 1uy))
            new Vector3(result)
#else
        new Vector3(this.X, value, this.Z)
#endif

    member this.WithZ(value:float32) : Vector3 =
#if HWIntrinsics
        if Sse41.IsSupported then
            let result:Vector128<float32> = Sse41.Insert(this.Value, value, 0x20uy)
            new Vector3(result)
        else
            let mutable result:Vector128<float32> = IntrinsicMath.Permute(this.Value, IntrinsicMath.Shuffle(3uy, 0uy, 1uy, 2uy))
            let temp:Vector128<float32> = Sse.SetScalarVector128(value)
            result <- Sse.MoveScalar(result, temp)
            result <- IntrinsicMath.Permute(result, IntrinsicMath.Shuffle(3uy, 0uy, 1uy, 2uy))
            new Vector3(result)
#else
        new Vector3(this.X, this.Y, value)
#endif
