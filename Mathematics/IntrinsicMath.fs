namespace DemoApplication.Mathematics

open System
open System.Runtime.Intrinsics
open System.Runtime.Intrinsics.X86

#if HWIntrinsics
module IntrinsicMath =
    let public Select:float32 = BitConverter.Int32BitsToSingle(0xFFFFFFFF)

    let public Mask3:Vector128<float32> = Sse.SetVector128(0.0f, Select, Select, Select)

    let inline public Shuffle(fp3:byte, fp2:byte, fp1:byte, fp0:byte) : byte =
        (fp3 <<< 6) ||| (fp2 <<< 4) ||| (fp1 <<< 2) ||| fp0

    let inline public Permute(value:Vector128<float32>, control:byte) : Vector128<float32> =
        if Avx.IsSupported then
            Avx.Permute(value, control)
        else
            Sse.Shuffle(value, value, control)

    let inline public DotVector3(left:Vector128<float32>, right:Vector128<float32>) : Vector128<float32> =
        if Sse41.IsSupported then
            Sse41.DotProduct(left, right, 0x7Fuy)
        else if Sse3.IsSupported then
            let mutable temp:Vector128<float32> = Sse.Multiply(left, right)
            temp <- Sse.And(temp, Mask3)
            temp <- Sse3.HorizontalAdd(temp, temp)
            Sse3.HorizontalAdd(temp, temp)
        else
            let mutable dot:Vector128<float32> = Sse.Multiply(left, right)
            let mutable temp:Vector128<float32> = Permute(dot, Shuffle(2uy, 1uy, 2uy, 1uy))
            dot <- Sse.AddScalar(dot, temp)
            temp <- Permute(temp, Shuffle(1uy, 1uy, 1uy, 1uy))
            dot <- Sse.AddScalar(dot, temp)
            Permute(dot, Shuffle(0uy, 0uy, 0uy, 0uy))

    let inline public DotVector4(left:Vector128<float32>, right:Vector128<float32>) : Vector128<float32> =
        if Sse41.IsSupported then
            Sse41.DotProduct(left, right, 0xFFuy)
        else if Sse3.IsSupported then
            let mutable temp:Vector128<float32> = Sse.Multiply(left, right)
            temp <- Sse3.HorizontalAdd(temp, temp)
            Sse3.HorizontalAdd(temp, temp)
        else
            let mutable dot:Vector128<float32> = Sse.Multiply(left, right)
            let mutable temp:Vector128<float32> = Sse.Shuffle(right, dot, Shuffle(1uy, 0uy, 0uy, 0uy))
            temp <- Sse.AddScalar(temp, dot)
            dot <- Sse.Shuffle(dot, temp, Shuffle(0uy, 3uy, 0uy, 0uy))
            dot <- Sse.AddScalar(dot, temp)
            Permute(dot, Shuffle(2uy, 2uy, 2uy, 2uy))
#endif
