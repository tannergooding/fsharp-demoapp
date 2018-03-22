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
        let (top:Vector3, bottom:Vector3) =
            if point1.y <= point2.y then
                (point1, point2)
            else
                (point2, point1)

        let (deltaX:int32, deltaY:int32, direction:int32) = 
            let result = bottom - top

            if result.x >= 0.0f then
                (int32 result.x, int32 result.y, 1)
            else
                (-(int32 result.x), int32 result.y, -1)

        let mutable (posX:int32, posY:int32) = (int32 top.x, int32 top.y)

        if deltaX = 0 then
            for i = 0 to deltaY do
                this.DrawPixel(posX, posY, color)
                posY <- posY + 1
        else if deltaY = 0 then
            for i = 0 to deltaX do
                this.DrawPixel(posX, posY, color)
                posX <- posX + direction
        else if deltaX = deltaY then
            for i = 0 to deltaX do
                this.DrawPixel(posX, posY, color)
                posX <- posX + direction
                posY <- posY + 1
        else if deltaX > deltaY then
            let pixelsPerStep:int32 = (deltaX / deltaY)
            let adjustUp:int32 = (deltaX % deltaY) * 2
            let adjustDown:int32 = (deltaY * 2)

            let initialPixelStep:int32 = 
                if (adjustUp = 0) && ((pixelsPerStep &&& 1) = 0) then
                    (pixelsPerStep / 2)
                else
                    (pixelsPerStep / 2) + 1

            let mutable errorTerm:int32 =
                if (pixelsPerStep &&& 1) <> 0 then
                    (deltaX % deltaY) - deltaY
                else
                    (deltaX % deltaY) - (deltaY * 2)

            for pixelsDrawn = 0 to (initialPixelStep - 1) do
                this.DrawPixel(posX, posY, color)
                posX <- posX + direction
            posY <- posY + 1

            for i = 0 to (deltaY - 2) do
                let mutable runLength:int32 = pixelsPerStep
                errorTerm <- errorTerm + adjustUp

                if errorTerm > 0 then
                    runLength <- runLength + 1
                    errorTerm <- errorTerm - adjustDown

                for pixelsDrawn = 0 to (runLength - 1) do
                    this.DrawPixel(posX, posY, color)
                    posX <- posX + direction
                posY <- posY + 1

            for pixelsDrawn = 0 to (initialPixelStep - 1) do
                this.DrawPixel(posX, posY, color)
                posX <- posX + direction
        else
            let pixelsPerStep:int32 = (deltaY / deltaX)
            let adjustUp:int32 = (deltaY % deltaX) * 2
            let adjustDown:int32 = (deltaX * 2)

            let initialPixelStep:int32 = 
                if (adjustUp = 0) && ((pixelsPerStep &&& 1) = 0) then
                    (pixelsPerStep / 2)
                else
                    (pixelsPerStep / 2) + 1

            let mutable errorTerm:int32 =
                if (pixelsPerStep &&& 1) <> 0 then
                    (deltaY % deltaX) - deltaX
                else
                    (deltaY % deltaX) - (deltaX * 2)

            for pixelsDrawn = 0 to (initialPixelStep - 1) do
                this.DrawPixel(posX, posY, color)
                posY <- posY + 1
            posX <- posX + direction

            for i = 0 to (deltaX - 2) do
                let mutable runLength:int32 = pixelsPerStep
                errorTerm <- errorTerm + adjustUp

                if errorTerm > 0 then
                    runLength <- runLength + 1
                    errorTerm <- errorTerm - adjustDown

                for pixelsDrawn = 0 to (runLength - 1) do
                    this.DrawPixel(posX, posY, color)
                    posY <- posY + 1
                posX <- posX + direction

            for pixelsDrawn = 0 to (initialPixelStep - 1) do
                this.DrawPixel(posX, posY, color)
                posY <- posY + 1

    member public this.DrawPixel(x:int32, y:int32, color:uint32) : unit =
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
