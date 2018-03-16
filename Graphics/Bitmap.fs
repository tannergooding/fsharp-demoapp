namespace DemoApplication.Graphics

open DemoApplication.Mathematics
open DemoApplication.Utilities
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

    member public this.DrawLine(point1:Vector3, point2:Vector3, color:uint32) : unit =
        let mutable xMin:float32 = point1.x
        let mutable xMax:float32 = point2.x
        let mutable yMin:float32 = point1.y
        let mutable yMax:float32 = point2.y

        if point1.y > point2.y then
            xMin <- point2.x
            xMax <- point1.x

            yMin <- point2.y
            yMax <- point1.y

        let mutable xDelta:int32 = int32 (xMax - xMin)
        let yDelta:int32 = int32 (yMax - yMin)
        let mutable direction:int32 = 1

        if xDelta < 0 then
            xDelta <- -xDelta
            direction <- -1

        let mutable xPos:float32 = xMin
        let mutable yPos:float32 = yMin

        if xDelta = 0 then
            for i = 0 to yDelta do
                this.DrawPixel(int32 xPos, int32 yPos, color)
                yPos <- yPos + 1.0f
        else if yDelta = 0 then
            for i = 0 to xDelta do
                this.DrawPixel(int32 xPos, int32 yPos, color)
                xPos <- xPos + float32 direction
        else if xDelta = yDelta then
            for i = 0 to xDelta do
                this.DrawPixel(int32 xPos, int32 yPos, color)
                xPos <- xPos + float32 direction
                yPos <- yPos + 1.0f
        else if xDelta > yDelta then
            let pixelsPerStep:int32 = (xDelta / yDelta)
            let mutable initialPixelStep:int32 = (pixelsPerStep / 2) + 1
            let adjustUp:int32 = (xDelta % yDelta) * 2
            let adjustDown:int32 = (yDelta * 2)
            let mutable errorTerm:int32 = (xDelta % yDelta) - (yDelta * 2)

            if (adjustUp = 0) && ((pixelsPerStep &&& 1) = 0) then
                initialPixelStep <- initialPixelStep- 1

            if (pixelsPerStep &&& 1) <> 0 then
                errorTerm <- errorTerm + yDelta

            for pixelsDrawn = 0 to (initialPixelStep - 1) do
                this.DrawPixel(int32 xPos, int32 yPos, color)
                xPos <- xPos + float32 direction
            yPos <- yPos + 1.0f

            for i = 0 to (yDelta - 2) do
                let mutable runLength:int32 = pixelsPerStep
                errorTerm <- errorTerm + adjustUp

                if errorTerm > 0 then
                    runLength <- runLength + 1
                    errorTerm <- errorTerm - adjustDown

                for pixelsDrawn = 0 to (runLength - 1) do
                    this.DrawPixel(int32 xPos, int32 yPos, color)
                    xPos <- xPos + float32 direction
                yPos <- yPos + 1.0f

            for pixelsDrawn = 0 to (initialPixelStep - 1) do
                this.DrawPixel(int32 xPos, int32 yPos, color)
                xPos <- xPos + float32 direction
        else
            let pixelsPerStep:int32 = (yDelta / xDelta)
            let mutable initialPixelStep:int32 = (pixelsPerStep / 2) + 1
            let adjustUp:int32 = (yDelta % xDelta) * 2
            let adjustDown:int32 = (xDelta * 2)
            let mutable errorTerm:int32 = (yDelta % xDelta) - (xDelta * 2)

            if (adjustUp = 0) && ((pixelsPerStep &&& 1) = 0) then
                initialPixelStep <- initialPixelStep- 1

            if (pixelsPerStep &&& 1) <> 0 then
                errorTerm <- errorTerm + yDelta

            for pixelsDrawn = 0 to (initialPixelStep - 1) do
                this.DrawPixel(int32 xPos, int32 yPos, color)
                yPos <- yPos + 1.0f
            xPos <- xPos + float32 direction

            for i = 0 to (xDelta - 2) do
                let mutable runLength:int32 = pixelsPerStep
                errorTerm <- errorTerm + adjustUp

                if errorTerm > 0 then
                    runLength <- runLength + 1
                    errorTerm <- errorTerm - adjustDown

                for pixelsDrawn = 0 to (runLength - 1) do
                    this.DrawPixel(int32 xPos, int32 yPos, color)
                    yPos <- yPos + 1.0f
                xPos <- xPos + float32 direction

            for pixelsDrawn = 0 to (initialPixelStep - 1) do
                this.DrawPixel(int32 xPos, int32 yPos, color)
                yPos <- yPos + 1.0f

    member public this.DrawPixel(x:int32, y:int32, color:uint32) : unit =
        let index:int32 = ((y * width) + x)

        if (index < 0) && (index >= pixelCount) then
            ExceptionUtilities.ThrowArgumentOutOfRangeException("index", index)

        this.DrawPixelUnsafe(index, color)

    member public this.DrawPolygon(polygon:Polygon) : unit =
        for i = 0 to (polygon.VerticeGroups.Count - 1) do
            if not (this.ShouldCull(polygon, i)) then
                match polygon.VerticeGroups.[i].Length with
                | 1 -> this.DrawPixel(int32 polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[0]].x, int32 polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[0]].y, 0u)
                | 2 -> this.DrawLine(polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[0]], polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[1]], 0u)
                | 3 -> this.DrawTriangle(polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[0]], polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[1]], polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[2]], 0u)
                | 4 -> this.DrawQuad(polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[0]], polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[1]], polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[2]], polygon.ModifiedVertices.[polygon.VerticeGroups.[i].[3]], 0u)
                | _ -> invalidOp "Bad VerticeGroup Length"

    member public this.DrawQuad(point1:Vector3, point2:Vector3, point3:Vector3, point4:Vector3, color:uint32) : unit =
        this.DrawLine(point1, point2, color)
        this.DrawLine(point2, point3, color)
        this.DrawLine(point3, point4, color)
        this.DrawLine(point4, point1, color)

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

    member internal this.DrawPixelUnsafe(index:int32, color:uint32) : unit =
        assert ((index >= 0) && (index < pixelCount))
        let buffer:nativeptr<uint32> = NativePtr.add (NativePtr.ofNativeInt<uint32> handle) index
        NativePtr.write<uint32> buffer color

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
