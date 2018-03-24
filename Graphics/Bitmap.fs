namespace DemoApplication.Graphics

open DemoApplication.Mathematics
open Microsoft.FSharp.NativeInterop
open System
open System.Runtime.InteropServices

type Bitmap public (width:int32, height:int32) =
    // Fields
    let bitsPerPixel:int32 = 32
    let planeCount:int32 = 1
    let mutable width:int32 = width
    let mutable height:int32 = height
    let mutable pixelCount:int32 = (width * height)
    let mutable length:int32 = (pixelCount * (bitsPerPixel / 8))
    let mutable handle:nativeint = Marshal.AllocHGlobal(nativeint length)

    // Properties
    member public this.BitsPerPixel with get() : int32 = bitsPerPixel
    member public this.Handle with get() : nativeint = handle
    member public this.Height with get() : int32 = height
    member public this.Length with get() : int32 = length
    member public this.PixelCount with get() : int32 = pixelCount
    member public this.PlaneCount with get() : int32 = planeCount
    member public this.Width with get() : int32 = width

    // Methods
    member public this.Clear(color:uint32) : unit =
        for index = 0 to (pixelCount - 1) do
            this.DrawPixelUnsafe(index, color)

    member public this.Dispose() : unit =
        this.Dispose true
        GC.SuppressFinalize(this)

    // This is based on "Bresenham’s Line  Generation Algorithm with Built-in Clipping - Yevgeny P. Kuzmin"
    member public this.DrawLine(point1:Vector3, point2:Vector3, color:uint32) : unit =
        if pixelCount <> 0 then
            // Draw from top to bottom to reduce the cases that need handled and to ensure
            // a deterministic line is drawn for the same endpoints. We also prefer drawing
            // from left to right, in the scenario where y1 = y2.
            let ((x1:int32, y1:int32), (x2:int32, y2:int32)) =
                let (x1:int32, y1:int32) = (int32 point1.x, int32 point1.y)
                let (x2:int32, y2:int32) = (int32 point2.x, int32 point2.y)

                if (y1 < y2) || ((y1 = y2) && (x1 < x2)) then
                    ((x1, y1), (x2, y2))
                else
                    ((x2, y2), (x1, y1))

            if x1 = x2 then
                if y1 = y2 then
                    this.DrawPixel(x1, y1, color)
                else
                    this.DrawVerticalLine(x1, y1, y2, color)
            else if y1 = y2 then
                this.DrawHorizontalLine(x1, y1, x2, color)
            else if x1 < x2 then
                this.DrawDiagonalLineLeftToRight(x1, y1, x2, y2, color)
            else
                this.DrawDiagonalLineRightToLeft(x1, y1, x2, y2, color)

    member public this.DrawPixel(x:int32, y:int32, color:uint32) : unit =
        let (width:int32, height:int32) = (width, height)

        if ((x >= 0) && (x < width) && (y >= 0) && (y < height)) then
            let index:int32 = ((y * width) + x)
            this.DrawPixelUnsafe(index, color)

    member public this.DrawPolygon(polygon:Polygon, isTriangles:bool, isCulling:bool) : unit =
        for i = 0 to (polygon.VerticeGroups.Count - 1) do
            if not isCulling || not (this.ShouldCull(polygon, i)) then
                match polygon.VerticeGroups.[i].Length with
                | 1 -> this.DrawPixel(int32 polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[0]].x, int32 polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[0]].y, 0u)
                | 2 -> this.DrawLine(polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[0]], polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[1]], 0u)
                | 3 -> this.DrawTriangle(polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[0]], polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[1]], polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[2]], 0u)
                | 4 -> this.DrawQuad(polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[0]], polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[1]], polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[2]], polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[3]], 0u, isTriangles)
                | _ -> invalidOp "Bad VerticeGroup Length"

    member public this.DrawQuad(point1:Vector3, point2:Vector3, point3:Vector3, point4:Vector3, color:uint32, isTriangles:bool) : unit =
        this.DrawLine(point1, point2, color)
        this.DrawLine(point2, point3, color)
        this.DrawLine(point3, point4, color)
        this.DrawLine(point4, point1, color)
        if isTriangles then this.DrawLine(point1, point3, color)

    member public this.DrawTriangle(point1:Vector3, point2:Vector3, point3:Vector3, color:uint32) : unit =
        this.DrawLine(point1, point2, color)
        this.DrawLine(point2, point3, color)
        this.DrawLine(point3, point1, color)

    member public this.Resize(newWidth:int32, newHeight:int32) : unit =
        let previousPixelCount = pixelCount

        width <- newWidth
        height <- newHeight
        pixelCount <- (width * height)

        if pixelCount <> previousPixelCount then
            length <- (pixelCount * (bitsPerPixel / 8))
            handle <- Marshal.ReAllocHGlobal(handle, nativeint length)

    member internal this.DrawDiagonalLineInternal(d1:int32 byref, d2:int32 byref, sx1:int32, sy1:int32, sx2:int32, sy2:int32, dsx:int32, dsy:int32, wx1:int32, wy1:int32, wx2:int32, wy2:int32, stx:int32, sty:int32, xd:int32 byref, yd:int32 byref, color:uint32) =
        // Some general assertions that the paper maintains
        assert ((wx1 <= wx2) && (wy1 <= wy2))
        assert ((sx1 <= sx2) && (sy1 <= sy2))
        assert (dsy <= dsx)

        let mutable (reject:bool, setExit:bool) = (false, false)

        let mutable (dx2:int32, dy2:int32) = ((2 * dsx), (2 * dsy))
        xd <- sx1
        yd <- sy1
        let mutable (e:int32, term:int32) = (((2 * dsy) - dsx), sx2)

        if sy1 < wy1 then                           // Horizontal Entry
            let tmp:int32 = (dx2 * (wy1 - sy1)) - dsx
            xd <- xd + (tmp / dy2)
            let rem:int32 = tmp % dy2

            if xd > wx2 then
                reject <- true
            else if (xd + 1) >= wx1 then
                yd <- wy1
                e <- e - (rem + dsx)

                if rem > 0 then
                    xd <- xd + 1
                    e <- e + dy2

                setExit <- true

        if not reject then
            if (not setExit) && (sx1 < wx1) then    // Vertical Entry
                let tmp:int32 = dy2 * (wx1 - sx1)
                yd <- yd + (tmp / dx2)
                let rem:int32 = tmp % dx2

                if (yd > wy2) || ((yd = wy2) && (rem >= dsx)) then
                    reject <- true
                else
                    xd <- wx1
                    e <- e + rem

                    if rem >= dsx then
                        yd <- yd + 1
                        e <- e - dx2

            if not reject then
                if sy2 > wy2 then                   // Exit
                    let tmp:int32 = dx2 * (wy2 - sy1) + dsx
                    term <- sx1 + (tmp / dy2)
                    let rem:int32 = tmp % dy2

                    if rem = 0 then
                        term <- term - 1

                if term > wx2 then
                    term <- wx2

                term <- term + 1

                if sty = -1 then
                    yd <- -yd                       // Reverse Transformation

                if stx = -1 then
                    xd <- -xd                       // Reverse Transformation
                    term <- -term

                dx2 <- dx2 - dy2

                while xd <> term do                 // Bresenham's Line Drawing
                    let index:int32 = (d2 * width) + d1
                    this.DrawPixelUnsafe(index, color)

                    if e >= 0 then
                        xd <- xd + stx
                        yd <- yd + sty
                        e <- e - dx2
                    else
                        xd <- xd + stx
                        e <- e + dy2

    member internal this.DrawDiagonalLineLeftToRight(x1:int32, y1:int32, x2:int32, y2:int32, color:uint32) : unit =
        // We only support drawing top to bottom and left to right; We also expect
        // the horizontal and vertical cases to have already been handled
        assert ((x1 < x2) && (y1 < y2))

        let (width:int32, height:int32) = (width, height)

        if (x2 >= 0) && (x1 < width) && (y2 >= 0) && (y1 < height) then
            let (sx1:int32, sy1:int32) = (x1, y1)
            let (sx2:int32, sy2:int32) = (x2, y2)

            let (stx:int32, sty:int32) = (1, 1)

            let (wx1:int32, wy1:int32) = (0, 0)
            let (wx2:int32, wy2:int32) = ((width - 1), (height - 1))

            let (dsx:int32, dsy:int32) = ((sx2 - sx1), (sy2 - sy1))

            let mutable (xd:int32, yd:int32) = (0, 0)

            if dsx >= dsy then      // Primarily Horizontal Line
                this.DrawDiagonalLineInternal(&xd, &yd, sx1, sy1, sx2, sy2, dsx, dsy, wx1, wy1, wx2, wy2, stx, sty, &xd, &yd, color)
            else                    // Primarily Vertical Line
                this.DrawDiagonalLineInternal(&yd, &xd, sy1, sx1, sy2, sx2, dsy, dsx, wy1, wx1, wy2, wx2, sty, stx, &xd, &yd, color)

    member internal this.DrawDiagonalLineRightToLeft(x1:int32, y1:int32, x2:int32, y2:int32, color:uint32) : unit =
        // We only support drawing top to bottom and left to right; We also expect
        // the horizontal and vertical cases to have already been handled
        assert ((x1 > x2) && (y1 < y2))

        let (width:int32, height:int32) = (width, height)

        if (x1 >= 0) && (x2 < width) && (y2 >= 0) && (y1 < height) then
            let (sx1:int32, sy1:int32) = (-x1, y1)
            let (sx2:int32, sy2:int32) = (-x2, y2)

            let (stx:int32, sty:int32) = (-1, 1)

            let (wx1:int32, wy1:int32) = (-(width - 1), 0)
            let (wx2:int32, wy2:int32) = (0, (height - 1))

            let (dsx:int32, dsy:int32) = ((sx2 - sx1), (sy2 - sy1))

            let mutable (xd:int32, yd:int32) = (0, 0)

            if dsx >= dsy then      // Primarily Horizontal Line
                this.DrawDiagonalLineInternal(&xd, &yd, sx1, sy1, sx2, sy2, dsx, dsy, wx1, wy1, wx2, wy2, stx, sty, &xd, &yd, color)
            else                    // Primarily Vertical Line
                this.DrawDiagonalLineInternal(&yd, &xd, sy1, sx1, sy2, sx2, dsy, dsx, wy1, wx1, wy2, wx2, sty, stx, &xd, &yd, color)

    member internal this.DrawHorizontalLine(x1:int32, y:int32, x2:int32, color:uint32) : unit =
        // We only support drawing left to right and expect the pixel case to have been handled
        assert (x1 < x2)

        let (width:int32, height:int32) = (width, height)

        if (y >= 0) && (y < height) && (x2 >= 0) && (x1 < width) then
            let startX:int32 = max x1 0
            let endX:int32 = min x2 (width - 1)

            let delta:int32 = endX - startX
            assert (delta >= 0)

            let mutable index:int32 = (y * width) + startX

            for i = 0 to delta do
                this.DrawPixelUnsafe(index, color)
                index <- index + 1

    member internal this.DrawPixelUnsafe(index:int32, color:uint32) : unit =
        assert ((index >= 0) && (index < pixelCount))
        let buffer:nativeptr<uint32> = NativePtr.add (NativePtr.ofNativeInt<uint32> handle) index
        NativePtr.write<uint32> buffer color

    member internal this.DrawVerticalLine(x:int32, y1:int32, y2:int32, color:uint32) : unit =
        // We only support drawing top to bottom and expect the pixel case to have been handled
        assert (y1 < y2)

        let (width:int32, height:int32) = (width, height)

        if (x >= 0) && (x < width) && (y2 >= 0) && (y1 < height) then
            let startY:int32 = max y1 0
            let endY:int32 = min y2 (height - 1)

            let delta:int32 = endY - startY
            assert (delta >= 0)

            let mutable index:int32 = (startY * width) + x

            for i = 0 to delta do
                this.DrawPixelUnsafe(index, color)
                index <- index + width

    member internal this.Dispose (isDisposing:bool) : unit =
        Marshal.FreeHGlobal(handle)

    member internal this.ShouldCull(polygon:Polygon, index:int32) : bool =
        let mutable normal = match polygon.NormalGroups.[index].Length with
                             | 1 -> polygon.ModifiedNormals.[polygon.NormalGroups.[index].[0]]
                             | 2 -> polygon.ModifiedNormals.[polygon.NormalGroups.[index].[1]]
                             | 3 -> polygon.ModifiedNormals.[polygon.NormalGroups.[index].[2]]
                             | 4 -> polygon.ModifiedNormals.[polygon.NormalGroups.[index].[3]]
                             | _ -> invalidOp "Bad NormalGroup Length"
        
        if index <> 1 then
            normal <- normal + match polygon.NormalGroups.[index].Length with
                               | 2 -> polygon.ModifiedNormals.[polygon.NormalGroups.[index].[0]]
                               | 3 -> polygon.ModifiedNormals.[polygon.NormalGroups.[index].[1]]
                               | _ -> polygon.ModifiedNormals.[polygon.NormalGroups.[index].[2]]
        
        if index <> 2 then
            normal <- normal + match polygon.NormalGroups.[index].Length with
                               | 3 -> polygon.ModifiedNormals.[polygon.NormalGroups.[index].[0]]
                               | _ -> polygon.ModifiedNormals.[polygon.NormalGroups.[index].[1]]
        
        if index <> 3 then
            normal <- normal + polygon.ModifiedNormals.[polygon.NormalGroups.[index].[0]]
        
        normal.Normalize() |> this.ShouldCull

    member internal this.ShouldCull(normal:Vector3) : bool =
        Vector3.DotProduct(normal, Vector3.UnitZ) >= 0.0f

    // System.IDisposable
    interface IDisposable with
        // Methods
        member this.Dispose() : unit = this.Dispose()

    // System.Object
    override this.Finalize() =
        this.Dispose false
