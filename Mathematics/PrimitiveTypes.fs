namespace rec DemoApplication.Mathematics

open System
open System.Runtime.Intrinsics
open System.Runtime.Intrinsics.X86
open System

module BoundingFrustumConstants =
    let HomogenousPoints:Vector4[] = [|
        new Vector4( 1.0f,  0.0f, 1.0f, 1.0f);
        new Vector4(-1.0f,  0.0f, 1.0f, 1.0f);
        new Vector4( 0.0f,  1.0f, 1.0f, 1.0f);
        new Vector4( 0.0f, -1.0f, 1.0f, 1.0f);
        new Vector4( 0.0f,  0.0f, 0.0f, 1.0f);
        new Vector4( 0.0f,  0.0f, 1.0f, 1.0f);
    |]

[<Struct>]
type BoundingFrustum =
    // Fields
    val public Origin:Vector3
    val public Orientation:Vector4
    val public RightSlope:float32
    val public LeftSlope:float32
    val public TopSlope:float32
    val public BottomSlope:float32
    val public Near:float32
    val public Far:float32

    // Constructors
    public new (origin:Vector3, orientation:Vector4, rightSlope:float32, leftSlope:float32, topSlope:float32, bottomSlope:float32, near:float32, far:float32) =
        { Origin = origin; Orientation = orientation; RightSlope = rightSlope; LeftSlope = leftSlope; TopSlope = topSlope; BottomSlope = bottomSlope; Near = near; Far = far }

    // Static Methods
    static member public CreateFrom(projection:Matrix4x4) : BoundingFrustum =
        let inverseProjection = projection.Invert()

        let points:Vector4[] = [|
            BoundingFrustumConstants.HomogenousPoints.[0].Transform(inverseProjection);
            BoundingFrustumConstants.HomogenousPoints.[1].Transform(inverseProjection);
            BoundingFrustumConstants.HomogenousPoints.[2].Transform(inverseProjection);
            BoundingFrustumConstants.HomogenousPoints.[3].Transform(inverseProjection);
            BoundingFrustumConstants.HomogenousPoints.[4].Transform(inverseProjection);
            BoundingFrustumConstants.HomogenousPoints.[5].Transform(inverseProjection);
        |]

        new BoundingFrustum(
            Vector3.Zero,
            Vector4.UnitW,
            (points.[0] / points.[0].Z).X,
            (points.[1] / points.[1].Z).X,
            (points.[2] / points.[2].Z).Y,
            (points.[3] / points.[3].Z).Y,
            (points.[4] / points.[4].W).Z,
            (points.[5] / points.[5].W).Z)

    // Methods
    member public this.Transform(transform:OrthogonalTransform) : BoundingFrustum =
        new BoundingFrustum(
            this.Origin.Transform(transform.Rotation) + transform.Translation,
            this.Orientation.Transform(transform.Rotation),
            this.RightSlope,
            this.LeftSlope,
            this.TopSlope,
            this.BottomSlope,
            this.Near,
            this.Far
        )

[<Struct>]
type Matrix3x3 =
    // Fields
    val public X:Vector3
    val public Y:Vector3
    val public Z:Vector3

    // Constructors
    public new (x:Vector3, y:Vector3, z:Vector3) =
        { X = x; Y = y; Z = z }

    // Static Properties
    static member public Identity with get() : Matrix3x3 = new Matrix3x3(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ)

    // Operators
    static member public (*) (left:Matrix3x3, right:Matrix3x3) : Matrix3x3 =
        let x:Vector3 = left.X.Transform(right)
        let y:Vector3 = left.Y.Transform(right)
        let z:Vector3 = left.Z.Transform(right)

        new Matrix3x3(x, y, z)

    // Static Methods
    static member public CreateFrom(rotation:Quaternion) : Matrix3x3 =
        let xx:float32 = rotation.X * rotation.X;
        let yy:float32 = rotation.Y * rotation.Y;
        let zz:float32 = rotation.Z * rotation.Z;
 
        let xy:float32 = rotation.X * rotation.Y;
        let wz:float32 = rotation.Z * rotation.W;
        let xz:float32 = rotation.Z * rotation.X;
        let wy:float32 = rotation.Y * rotation.W;
        let yz:float32 = rotation.Y * rotation.Z;
        let wx:float32 = rotation.X * rotation.W;
 
        new Matrix3x3(
            new Vector3((1.0f - 2.0f * (yy + zz)), (2.0f * (xy + wz)), (2.0f * (xz - wy))),
            new Vector3((2.0f * (xy - wz)), (1.0f - 2.0f * (zz + xx)), (2.0f * (yz + wx))),
            new Vector3((2.0f * (xz + wy)), (2.0f * (yz - wx)), (1.0f - 2.0f * (yy + xx)))
        )

[<Struct>]
type Matrix4x4 =
    // Fields
    val public X:Vector4
    val public Y:Vector4
    val public Z:Vector4
    val public W:Vector4

    // Constructors
    public new (x:Vector4, y:Vector4, z:Vector4, w:Vector4) =
        { X = x; Y = y; Z = z; W = w }

    // Static Properties
    static member public Identity with get() : Matrix4x4 = new Matrix4x4(Vector4.UnitX, Vector4.UnitY, Vector4.UnitZ, Vector4.UnitW)

    // Operators
    static member public (*) (left:Matrix4x4, right:Matrix4x4) : Matrix4x4 =
        let x:Vector4 = left.X.Transform(right)
        let y:Vector4 = left.Y.Transform(right)
        let z:Vector4 = left.Z.Transform(right)
        let w:Vector4 = left.W.Transform(right)

        new Matrix4x4(x, y, z, w)

    // Static Methods
    static member public CreateFrom(value:Matrix3x3, w:Vector3) =
        new Matrix4x4(
            new Vector4(value.X, 0.0f),
            new Vector4(value.Y, 0.0f),
            new Vector4(value.Z, 0.0f),
            new Vector4(w, 1.0f)
        )

    static member public CreateFrom(transform:OrthogonalTransform) : Matrix4x4 =
        Matrix4x4.CreateFrom(
            Matrix3x3.CreateFrom(transform.Rotation),
            transform.Translation
        )

    static member public CreateFrom(rotation:Quaternion) : Matrix4x4 =
        let xx:float32 = rotation.X * rotation.X;
        let yy:float32 = rotation.Y * rotation.Y;
        let zz:float32 = rotation.Z * rotation.Z;
 
        let xy:float32 = rotation.X * rotation.Y;
        let wz:float32 = rotation.Z * rotation.W;
        let xz:float32 = rotation.Z * rotation.X;
        let wy:float32 = rotation.Y * rotation.W;
        let yz:float32 = rotation.Y * rotation.Z;
        let wx:float32 = rotation.X * rotation.W;
 
        new Matrix4x4(
            new Vector4((1.0f - 2.0f * (yy + zz)), (2.0f * (xy + wz)), (2.0f * (xz - wy)), 0.0f),
            new Vector4((2.0f * (xy - wz)), (1.0f - 2.0f * (zz + xx)), (2.0f * (yz + wx)), 0.0f),
            new Vector4((2.0f * (xz + wy)), (2.0f * (yz - wx)), (1.0f - 2.0f * (yy + xx)), 0.0f),
            Vector4.UnitW
        )

    // Methods
    member public this.Invert() : Matrix4x4 =
        let a = this.X.X
        let b = this.X.Y
        let c = this.X.Z
        let d = this.X.W

        let e = this.Y.X
        let f = this.Y.Y
        let g = this.Y.Z
        let h = this.Y.W

        let i = this.Z.X
        let j = this.Z.Y
        let k = this.Z.Z
        let l = this.Z.W

        let m = this.W.X
        let n = this.W.Y
        let o = this.W.Z
        let p = this.W.W
 
        let kp_lo = (k * p) - (l * o)
        let jp_ln = (j * p) - (l * n)
        let jo_kn = (j * o) - (k * n)
        let ip_lm = (i * p) - (l * m)
        let io_km = (i * o) - (k * m)
        let in_jm = (i * n) - (j * m)
 
        let a11 = +((f * kp_lo) - (g * jp_ln) + (h * jo_kn))
        let a12 = -((e * kp_lo) - (g * ip_lm) + (h * io_km))
        let a13 = +((e * jp_ln) - (f * ip_lm) + (h * in_jm))
        let a14 = -((e * jo_kn) - (f * io_km) + (g * in_jm))
 
        let det = a * a11 + b * a12 + c * a13 + d * a14;
 
        if MathF.Abs(det) < Single.Epsilon then
            new Matrix4x4(
                new Vector4(Single.NaN),
                new Vector4(Single.NaN),
                new Vector4(Single.NaN),
                new Vector4(Single.NaN)
            )
        else
            let invDet = 1.0f / det
 
            let (xx, yx, zx, wx) = ((a11 * invDet),
                                    (a12 * invDet),
                                    (a13 * invDet),
                                    (a14 * invDet))
 
            let (xy, yy, zy, wy) = ((-((b * kp_lo) - (c * jp_ln) + (d * jo_kn)) * invDet),
                                    (+((a * kp_lo) - (c * ip_lm) + (d * io_km)) * invDet),
                                    (-((a * jp_ln) - (b * ip_lm) + (d * in_jm)) * invDet),
                                    (+((a * jo_kn) - (b * io_km) + (c * in_jm)) * invDet))
 
            let gp_ho = (g * p) - (h * o)
            let fp_hn = (f * p) - (h * n)
            let fo_gn = (f * o) - (g * n)
            let ep_hm = (e * p) - (h * m)
            let eo_gm = (e * o) - (g * m)
            let en_fm = (e * n) - (f * m)
 
            let (xz, yz, zz, wz) = ((+((b * gp_ho) - (c * fp_hn) + (d * fo_gn)) * invDet),
                                    (-((a * gp_ho) - (c * ep_hm) + (d * eo_gm)) * invDet),
                                    (+((a * fp_hn) - (b * ep_hm) + (d * en_fm)) * invDet),
                                    (-((a * fo_gn) - (b * eo_gm) + (c * en_fm)) * invDet))

 
            let gl_hk = (g * l) - (h * k)
            let fl_hj = (f * l) - (h * j)
            let fk_gj = (f * k) - (g * j)
            let el_hi = (e * l) - (h * i)
            let ek_gi = (e * k) - (g * i)
            let ej_fi = (e * j) - (f * i)
 
            let (xw, yw, zw, ww) = ((-((b * gl_hk) - (c * fl_hj) + (d * fk_gj)) * invDet),
                                    (+((a * gl_hk) - (c * el_hi) + (d * ek_gi)) * invDet),
                                    (-((a * fl_hj) - (b * el_hi) + (d * ej_fi)) * invDet),
                                    (+((a * fk_gj) - (b * ek_gi) + (c * ej_fi)) * invDet))

            new Matrix4x4(
                new Vector4(xx, xy, xz, xw),
                new Vector4(yx, yy, yz, yw),
                new Vector4(zx, zy, zz, zw),
                new Vector4(wx, wy, wz, ww)
            )

[<Struct>]
type OrthogonalTransform =
    // Fields
    val public Rotation:Quaternion
    val public Translation:Vector3

    // Constructors
    public new (rotation:Quaternion, translation:Vector3) =
        { Rotation = rotation; Translation = translation }

    // Static Properties
    static member public Identity with get() : OrthogonalTransform = new OrthogonalTransform(Quaternion.Identity, Vector3.Zero)

    // Methods
    member public this.Invert() : OrthogonalTransform =
        let inverseRotation = this.Rotation.Conjugate
        new OrthogonalTransform(
            inverseRotation,
            (-this.Translation).Transform(inverseRotation)
        )

    member public this.WithRotation(rotation:Quaternion) : OrthogonalTransform =
        new OrthogonalTransform(rotation, this.Translation)

    member public this.WithTranslation(translation:Vector3) : OrthogonalTransform =
        new OrthogonalTransform(this.Rotation, translation)

[<Struct>]
type Quaternion =
    // Fields
    val public Value:Vector4

    // Constructors
    public new (x:float32, y:float32, z:float32, w:float32) =
        { Value = new Vector4(x, y, z, w) }

    public new (value:Vector3, w:float32) =
        { Value = new Vector4(value, w) }

    public new (value:Vector4) =
        { Value = value }

#if HWIntrinsics
    public new (value:Vector128<float32>) =
        { Value = new Vector4(value) }
#endif

    // Static Properties
    static member public Identity with get() : Quaternion = new Quaternion(Vector4.UnitW)

    // Properties
    member public this.Conjugate with get() : Quaternion = new Quaternion(-this.X, -this.Y, -this.Z, this.W)

    member public this.X with get() : float32 = this.Value.X
    member public this.Y with get() : float32 = this.Value.Y
    member public this.Z with get() : float32 = this.Value.Z
    member public this.W with get() : float32 = this.Value.W

    // Static Methods
    static member public CreateFrom(pitch:float32, yaw:float32, roll:float32) =
        let halfPitch = pitch * 0.5f
        let sp = MathF.Sin(halfPitch)
        let cp = MathF.Cos(halfPitch)

        let halfYaw = yaw * 0.5f
        let sy = MathF.Sin(halfYaw)
        let cy = MathF.Cos(halfYaw)

        let halfRoll = roll * 0.5f
        let sr = MathF.Sin(halfRoll)
        let cr = MathF.Cos(halfRoll)

        new Quaternion(
            ((cy * sp) * cr) + ((sy * cp) * sr),
            ((sy * cp) * cr) - ((cy * sp) * sr),
            ((cy * cp) * sr) - ((sy * sp) * cr),
            ((cy * cp) * cr) + ((sy * sp) * sr)
        )

    static member public CreateFrom(value:Matrix3x3) : Quaternion =
        let trace = value.X.X + value.Y.Y + value.Z.Z;
 
        if trace > 0.0f then
            let s = MathF.Sqrt(trace + 1.0f)
            let invS = 0.5f / s
            new Quaternion(
                (value.Y.Z - value.Z.Y) * invS,
                (value.Z.X - value.X.Z) * invS,
                (value.X.Y - value.Y.X) * invS,
                s * 0.5f
            )
        else if (value.X.X >= value.Y.Y) && (value.X.X >= value.Z.Z) then
            let s = MathF.Sqrt(1.0f + value.X.X - value.Y.Y - value.Z.Z)
            let invS = 0.5f / s
            new Quaternion(
                0.5f * s,
                (value.X.Y + value.Y.X) * invS,
                (value.X.Z + value.Z.X) * invS,
                (value.Y.Z - value.Z.Y) * invS
            )
        else if value.Y.Y > value.Z.Z then
            let s = MathF.Sqrt(1.0f + value.Y.Y - value.X.X - value.Z.Z)
            let invS = 0.5f / s
            new Quaternion(
                (value.Y.X + value.X.Y) * invS,
                0.5f * s,
                (value.Z.Y + value.Y.Z) * invS,
                (value.Z.X - value.X.Z) * invS
            )
        else
            let s = MathF.Sqrt(1.0f + value.Z.Z - value.X.X - value.Y.Y)
            let invS = 0.5f / s
            new Quaternion(
                (value.Z.X + value.X.Z) * invS,
                (value.Z.Y + value.Y.Z) * invS,
                0.5f * s,
                (value.X.Y - value.Y.X) * invS
            )

    // Methods
    member public this.Normalize() : Quaternion =
        new Quaternion(this.Value.Normalize())

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
    static member public Zero with get() : Vector3 = new Vector3()

    static member public UnitX with get() : Vector3 = new Vector3(1.0f, 0.0f, 0.0f)
    static member public UnitY with get() : Vector3 = new Vector3(0.0f, 1.0f, 0.0f)
    static member public UnitZ with get() : Vector3 = new Vector3(0.0f, 0.0f, 1.0f)

    static member public One with get() : Vector3 = new Vector3(1.0f)

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
    static member (~-) (value:Vector3) =
        Vector3.Zero - value

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
    static member CrossProduct(left:Vector3, right:Vector3) : Vector3 =
        new Vector3(
            (left.Y * right.Z) - (left.Z * right.Y),
            (left.Z * right.X) - (left.X * right.Z),
            (left.X * right.Y) - (left.Y * right.X)
        )

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

    member public this.Transform(transformation:Matrix3x3) : Vector3 =
#if HWIntrinsics
        let mutable xx:Vector128<float32> = IntrinsicMath.Permute(this.Value, IntrinsicMath.Shuffle(0uy, 0uy, 0uy, 0uy))
        let mutable xy:Vector128<float32> = IntrinsicMath.Permute(this.Value, IntrinsicMath.Shuffle(1uy, 1uy, 1uy, 1uy))
        let mutable xz:Vector128<float32> = IntrinsicMath.Permute(this.Value, IntrinsicMath.Shuffle(2uy, 2uy, 2uy, 2uy))

        xx <- Sse.Multiply(xx, transformation.X.Value)
        xy <- Sse.Multiply(xy, transformation.Y.Value)
        xz <- Sse.Multiply(xz, transformation.Z.Value)

        let mutable temp:Vector128<float32> = Sse.Add(xx, xy)
        temp <- Sse.Add(temp, xz)
        new Vector3(temp)
#else
        new Vector3((this.X * transformation.X.X) + (this.Y * transformation.Y.X) + (this.Z * transformation.Z.X),
                    (this.X * transformation.X.Y) + (this.Y * transformation.Y.Y) + (this.Z * transformation.Z.Y),
                    (this.X * transformation.X.Z) + (this.Y * transformation.Y.Z) + (this.Z * transformation.Z.Z))
#endif

    member public this.Transform(transformation:Matrix4x4) : Vector3 =
#if HWIntrinsics
        let mutable xx:Vector128<float32> = IntrinsicMath.Permute(this.Value, IntrinsicMath.Shuffle(0uy, 0uy, 0uy, 0uy))
        let mutable xy:Vector128<float32> = IntrinsicMath.Permute(this.Value, IntrinsicMath.Shuffle(1uy, 1uy, 1uy, 1uy))
        let mutable xz:Vector128<float32> = IntrinsicMath.Permute(this.Value, IntrinsicMath.Shuffle(2uy, 2uy, 2uy, 2uy))

        xx <- Sse.Multiply(xx, transformation.X.Value)
        xy <- Sse.Multiply(xy, transformation.Y.Value)
        xz <- Sse.Multiply(xz, transformation.Z.Value)

        let mutable temp:Vector128<float32> = Sse.Add(xx, xy)
        temp <- Sse.Add(temp, xz)
        new Vector3(temp)
#else
        new Vector3((this.X * transformation.X.X) + (this.Y * transformation.Y.X) + (this.Z * transformation.Z.X),
                    (this.X * transformation.X.Y) + (this.Y * transformation.Y.Y) + (this.Z * transformation.Z.Y),
                    (this.X * transformation.X.Z) + (this.Y * transformation.Y.Z) + (this.Z * transformation.Z.Z))
#endif

    member this.Transform(rotation:Quaternion) : Vector3 =
        let x2:float32 = rotation.X + rotation.X;
        let y2:float32 = rotation.Y + rotation.Y;
        let z2:float32 = rotation.Z + rotation.Z;
 
        let wx2:float32 = rotation.W * x2;
        let wy2:float32 = rotation.W * y2;
        let wz2:float32 = rotation.W * z2;
        let xx2:float32 = rotation.X * x2;
        let xy2:float32 = rotation.X * y2;
        let xz2:float32 = rotation.X * z2;
        let yy2:float32 = rotation.Y * y2;
        let yz2:float32 = rotation.Y * z2;
        let zz2:float32 = rotation.Z * z2;
 
        new Vector3(
            (this.X * (1.0f - yy2 - zz2)) + (this.Y * (xy2 - wz2)) + (this.Z * (xz2 + wy2)),
            (this.X * (xy2 + wz2)) + (this.Y * (1.0f - xx2 - zz2)) + (this.Z * (yz2 - wx2)),
            (this.X * (xz2 - wy2)) + (this.Y * (yz2 + wx2)) + (this.Z * (1.0f - xx2 - yy2))
        )

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

[<Struct>]
type Vector4 =
    // Fields
#if HWIntrinsics
    val public Value:Vector128<float32>
#else
    val public X:float32
    val public Y:float32
    val public Z:float32
    val public W:float32
#endif

    // Constructors
    public new (value:float32) =
#if HWIntrinsics
        { Value = Sse.SetAllVector128(value) }
#else
        { X = value; Y = value; Z = value; W = value }
#endif

    public new (x:float32, y:float32, z:float32, w:float32) =
#if HWIntrinsics
        { Value = Sse.SetVector128(w, z, y, x) }
#else
        { X = x; Y = y; Z = z; W = w }
#endif

#if HWIntrinsics
    public new (value:Vector128<float32>) =
        { Value = value }
#endif

    public new (value:Vector3, w:float32) =
#if HWIntrinsics
        { Value = Sse.SetVector128(w, value.Z, value.Y, value.X) }
#else
        { X = value.X; Y = value.Y; Z = value.Z; W = w }
#endif

    // Static Properties
    static member public Zero with get() : Vector4 = new Vector4()

    static member public UnitX with get() : Vector4 = new Vector4(1.0f, 0.0f, 0.0f, 0.0f)
    static member public UnitY with get() : Vector4 = new Vector4(0.0f, 1.0f, 0.0f, 0.0f)
    static member public UnitZ with get() : Vector4 = new Vector4(0.0f, 0.0f, 1.0f, 0.0f)
    static member public UnitW with get() : Vector4 = new Vector4(0.0f, 0.0f, 0.0f, 1.0f)

    static member public One with get() : Vector4 = new Vector4(1.0f)

    // Properties
#if HWIntrinsics
    member public this.Length with get() : float32 = let mutable temp:Vector128<float32> = IntrinsicMath.DotVector4(this.Value, this.Value)
                                                     temp <- Sse.Sqrt(temp)
                                                     Sse.ConvertToSingle(temp)

    member public this.LengthSquared with get() : float32 = let temp:Vector128<float32> = IntrinsicMath.DotVector4(this.Value, this.Value)
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

    member public this.W with get() : float32 = if Sse41.IsSupported then
                                                    Sse41.Extract(this.Value, 3uy)
                                                else
                                                    let temp = IntrinsicMath.Permute(this.Value, IntrinsicMath.Shuffle(3uy, 3uy, 3uy, 3uy))
                                                    Sse.ConvertToSingle(temp)
#else
    member public this.Length with get() : float32 = MathF.Sqrt(this.LengthSquared)

    member public this.LengthSquared with get() : float32 = Vector4.DotProduct(this, this)
#endif

    // Operators
    static member (~-) (value:Vector4) =
        Vector4.Zero - value

    static member (+) (left:Vector4, right:Vector4) =
#if HWIntrinsics
        let result:Vector128<float32> = Sse.Add(left.Value, right.Value)
        new Vector4(result)
#else
        new Vector4(left.X + right.X,
                    left.Y + right.Y,
                    left.Z + right.Z,
                    left.W + right.W)
#endif

    static member (-) (left:Vector4, right:Vector4) =
#if HWIntrinsics
        let result:Vector128<float32> = Sse.Subtract(left.Value, right.Value)
        new Vector4(result)
#else
        new Vector4(left.X - right.X,
                    left.Y - right.Y,
                    left.Z - right.Z,
                    left.W - right.W)
#endif

    static member (*) (left:Vector4, right:float32) =
#if HWIntrinsics
        left * new Vector4(right)
#else
        new Vector4(left.X * right,
                    left.Y * right,
                    left.Z * right,
                    left.W * right)
#endif

    static member (*) (left:Vector4, right:Vector4) =
#if HWIntrinsics
        let result:Vector128<float32> = Sse.Multiply(left.Value, right.Value)
        new Vector4(result)
#else
        new Vector4(left.X * right.X,
                    left.Y * right.Y,
                    left.Z * right.Z,
                    left.W * right.W)
#endif

    static member (/) (left:Vector4, right:float32) : Vector4 =
#if HWIntrinsics
        left / new Vector4(right)
#else
        new Vector4(left.X / right,
                    left.Y / right,
                    left.Z / right,
                    left.W / right)
#endif

    static member (/) (left:Vector4, right:Vector4) : Vector4 =
#if HWIntrinsics
        let result:Vector128<float32> = Sse.Divide(left.Value, right.Value)
        new Vector4(result)
#else
        new Vector4(left.X / right.X,
                    left.Y / right.Y,
                    left.Z / right.Z,
                    left.W / right.W)
#endif

    // Static Methods
    static member DotProduct(left:Vector4, right:Vector4) : float32 =
#if HWIntrinsics
        let result:Vector128<float32> = IntrinsicMath.DotVector4(left.Value, right.Value)
        Sse.ConvertToSingle(result)
#else
        (left.X * right.X) +
        (left.Y * right.Y) +
        (left.Z * right.Z) +
        (left.W * right.W)
#endif

    // Methods
    member this.Normalize() : Vector4 =
#if HWIntrinsics
        let mutable length:Vector128<float32> = IntrinsicMath.DotVector4(this.Value, this.Value)
        length <- Sse.Sqrt(length)

        let result:Vector128<float32> = Sse.Divide(this.Value, length)
        new Vector4(result)
#else
        this / this.Length
#endif

    member this.Transform(transformation:Matrix4x4) : Vector4 =
#if HWIntrinsics
        let mutable xx:Vector128<float32> = IntrinsicMath.Permute(this.Value, IntrinsicMath.Shuffle(0uy, 0uy, 0uy, 0uy))
        let mutable xy:Vector128<float32> = IntrinsicMath.Permute(this.Value, IntrinsicMath.Shuffle(1uy, 1uy, 1uy, 1uy))
        let mutable xz:Vector128<float32> = IntrinsicMath.Permute(this.Value, IntrinsicMath.Shuffle(2uy, 2uy, 2uy, 2uy))
        let mutable xw:Vector128<float32> = IntrinsicMath.Permute(this.Value, IntrinsicMath.Shuffle(3uy, 3uy, 3uy, 3uy))

        xx <- Sse.Multiply(xx, transformation.X.Value)
        xy <- Sse.Multiply(xy, transformation.Y.Value)
        xz <- Sse.Multiply(xz, transformation.Z.Value)
        xw <- Sse.Multiply(xw, transformation.W.Value)

        let mutable temp:Vector128<float32> = Sse.Add(xx, xy)
        temp <- Sse.Add(temp, xz)
        temp <- Sse.Add(temp, xw)
        new Vector4(temp)
#else
        new Vector4((this.X * transformation.X.X) + (this.Y * transformation.Y.X) + (this.Z * transformation.Z.X) + (this.W * transformation.W.X),
                    (this.X * transformation.X.Y) + (this.Y * transformation.Y.Y) + (this.Z * transformation.Z.Y) + (this.W * transformation.W.Y),
                    (this.X * transformation.X.Z) + (this.Y * transformation.Y.Z) + (this.Z * transformation.Z.Z) + (this.W * transformation.W.Z),
                    (this.X * transformation.X.W) + (this.Y * transformation.Y.W) + (this.Z * transformation.Z.W) + (this.W * transformation.W.W))
#endif

    member this.Transform(rotation:Quaternion) : Vector4 =
        let x2:float32 = rotation.X + rotation.X;
        let y2:float32 = rotation.Y + rotation.Y;
        let z2:float32 = rotation.Z + rotation.Z;
 
        let wx2:float32 = rotation.W * x2;
        let wy2:float32 = rotation.W * y2;
        let wz2:float32 = rotation.W * z2;
        let xx2:float32 = rotation.X * x2;
        let xy2:float32 = rotation.X * y2;
        let xz2:float32 = rotation.X * z2;
        let yy2:float32 = rotation.Y * y2;
        let yz2:float32 = rotation.Y * z2;
        let zz2:float32 = rotation.Z * z2;
 
        new Vector4(
            (this.X * (1.0f - yy2 - zz2)) + (this.Y * (xy2 - wz2)) + (this.Z * (xz2 + wy2)),
            (this.X * (xy2 + wz2)) + (this.Y * (1.0f - xx2 - zz2)) + (this.Z * (yz2 - wx2)),
            (this.X * (xz2 - wy2)) + (this.Y * (yz2 + wx2)) + (this.Z * (1.0f - xx2 - yy2)),
            this.W
        )

    member this.WithX(value:float32) : Vector4 =
#if HWIntrinsics
        let mutable result:Vector128<float32> = Sse.SetScalarVector128(value)
        result <- Sse.MoveScalar(this.Value, result)
        new Vector4(result)
#else
        new Vector4(value, this.Y, this.Z, this.W)
#endif

    member this.WithY(value:float32) : Vector4 =
#if HWIntrinsics
        if Sse41.IsSupported then
            let result:Vector128<float32> = Sse41.Insert(this.Value, value, 0x10uy)
            new Vector4(result)
        else
            let mutable result:Vector128<float32> = IntrinsicMath.Permute(this.Value, IntrinsicMath.Shuffle(3uy, 2uy, 0uy, 1uy))
            let temp:Vector128<float32> = Sse.SetScalarVector128(value)
            result <- Sse.MoveScalar(result, temp)
            result <- IntrinsicMath.Permute(result, IntrinsicMath.Shuffle(3uy, 2uy, 0uy, 1uy))
            new Vector4(result)
#else
        new Vector4(this.X, value, this.Z, this.W)
#endif

    member this.WithZ(value:float32) : Vector4 =
#if HWIntrinsics
        if Sse41.IsSupported then
            let result:Vector128<float32> = Sse41.Insert(this.Value, value, 0x20uy)
            new Vector4(result)
        else
            let mutable result:Vector128<float32> = IntrinsicMath.Permute(this.Value, IntrinsicMath.Shuffle(3uy, 0uy, 1uy, 2uy))
            let temp:Vector128<float32> = Sse.SetScalarVector128(value)
            result <- Sse.MoveScalar(result, temp)
            result <- IntrinsicMath.Permute(result, IntrinsicMath.Shuffle(3uy, 0uy, 1uy, 2uy))
            new Vector4(result)
#else
        new Vector4(this.X, this.Y, value, this.W)
#endif

    member this.WithW(value:float32) : Vector4 =
#if HWIntrinsics
        if Sse41.IsSupported then
            let result:Vector128<float32> = Sse41.Insert(this.Value, value, 0x30uy)
            new Vector4(result)
        else
            let mutable result:Vector128<float32> = IntrinsicMath.Permute(this.Value, IntrinsicMath.Shuffle(0uy, 2uy, 1uy, 3uy))
            let temp:Vector128<float32> = Sse.SetScalarVector128(value)
            result <- Sse.MoveScalar(result, temp)
            result <- IntrinsicMath.Permute(result, IntrinsicMath.Shuffle(0uy, 2uy, 1uy, 3uy))
            new Vector4(result)
#else
        new Vector4(this.X, this.Y, this.Z, value)
#endif
