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

    // This is based on L36-2.C from "Michael Abrash's Graphics Programming Black Book"
    member public this.DrawLine(point1:Vector3, point2:Vector3, color:uint32) : unit =
        // Draw from top to bottom to reduce the cases that need handled and to ensure
        // a deterministic line is drawn for the same endpoints
        let (topX:int32, topY:int32, bottomX:int32, bottomY:int32) =
            if point1.y < point2.y then
                (int32 point1.x, int32 point1.y, int32 point2.x, int32 point2.y)
            else
                (int32 point2.x, int32 point2.y, int32 point1.x, int32 point1.y)

        // Cache width and height locally, since they are mutable
        let (width:int32, height:int32) = (width, height)

        // We are visible if the line crosses through any part of the bitmap
        //  * The top X needs to be before the right of the bitmap
        //  * The bottom X needs to be after the left of the bitmap
        //  * The top or bottom Y needs to be before the bottom of the bitmap
        //  * The top or bottom Y needs to be after the top of the bitmap
        let isVisible = (topY <= height) && (bottomY >= 0) &&
                        ((topX <= width) || (bottomX <= width)) &&
                        ((topX >= 0) || (bottomX >= 0))

        if isVisible then
            // Clip the top if it exists outside the bitmap
            //  When either component of the top point exists outside the bounds
            //  of the bitmap, we need to determine the closest point that would
            //  be drawn that is still in bounds. To determine that point, we take
            //  the inverse of the opposite component and multiply it by the run length.
            let (startX:int32, startY:int32) =
                if topY < 0 then
                    let x:int32 = topX + ((-topY) * int32 (float32 (bottomX - topX) / float32 (bottomY - topY)))

                    if topX < 0 then
                        if x < 0 then
                            let y:int32 = topY + ((-topX) * int32 (float32 (bottomY - topY) / float32 (bottomX - topX)))
                            (0, y)
                        else
                            (x, 0)
                    else if topX >= width then
                        if x >= width then
                            let x:int32 = (width - 1)
                            let y:int32 = topY + ((x - topX) * int32 (float32 (bottomY - topY) / float32 (bottomX - topX)))
                            (x, y)
                        else
                            (x, 0)
                    else
                        (x, 0)
                else if topX < 0 then
                    let y:int32 = topY + ((-topX) * int32 (float32 (bottomY - topY) / float32 (bottomX - topX)))
                    (0, y)
                else if topX >= width then
                    let x:int32 = (width - 1)
                    let y:int32 = topY + ((x - topX) * int32 (float32 (bottomY - topY) / float32 (bottomX - topX)))
                    (x, y)
                else
                    (topX, topY)

            // Clip the bottom if it exists outside the bitmap
            //  When either component of the top point exists outside the bounds
            //  of the bitmap, we need to determine the closest point that would
            //  be drawn that is still in bounds. To determine that point, we take
            //  the inverse of the opposite component and multiply it by the run length.
            let (endX:int32, endY:int32) =
                if bottomY >= height then
                    let y:int32 = (height - 1)
                    let x:int32 = bottomX + ((y - bottomY) * int32 (float32 (topX - bottomX) / float32 (topY - bottomY)))

                    if bottomX < 0 then
                        if x < 0 then
                            let y:int32 = bottomY + ((-bottomX) * int32 (float32 (topY - bottomY) / float32 (topX - bottomX)))
                            (0, y)
                        else
                            (x, y)
                    else if bottomX >= width then
                        if x >= width then
                            let x:int32 = (width - 1)
                            let y:int32 = bottomY + ((x - bottomX) * int32 (float32 (topY - bottomY) / float32 (topX - bottomX)))
                            (x, y)
                        else
                            (x, y)
                    else
                        (x, y)
                else if bottomX < 0 then
                    let y:int32 = bottomY + ((-bottomX) * int32 (float32 (topY - bottomY) / float32 (topX - bottomX)))
                    (0, y)
                else if bottomX >= width then
                    let x:int32 = (width - 1)
                    let y:int32 = bottomY + ((x - bottomX) * int32 (float32 (topY - bottomY) / float32 (topX - bottomX)))
                    (x, y)
                else
                    (bottomX, bottomY)

            // We are visible if the line crosses through any part of the bitmap
            //  * The top X needs to be after the left and before the right of the bitmap
            //  * The bottom X needs to be after the left and before the right of the bitmap
            //  * The top Y needs to be after the top and before the bottom of the bitmap
            //  * The bottom Y needs to be after the top and before the bottom of the bitmap
            let isVisible = (startX >= 0) && (startX < width) &&
                            (startY >= 0) && (startY < height) &&
                            (endX >= 0) && (endX < width) &&
                            (endY >= 0) && (endY < height)

            if isVisible then
                // Determine if we are going left to right (direction = 1) or right to left (direction = -1)
                let (deltaX:int32, deltaY:int32, direction:int32) = 
                    let x:int32 = (endX - startX)
                    let y:int32 = (endY - startY)

                    if x >= 0 then
                        (x, y, 1)
                    else
                        (-x, y, -1)

                let mutable index:int32 = (startY * width) + startX

                if deltaX = 0 then
                    // Vertical Line
                    for i = 0 to deltaY do
                        this.DrawPixelUnsafe(index, color)
                        index <- index + width
                else if deltaY = 0 then
                    // Horizontal Line
                    for i = 0 to deltaX do
                        this.DrawPixelUnsafe(index, color)
                        index <- index + direction
                else if deltaX = deltaY then
                    // Diagonal Line
                    for i = 0 to deltaX do
                        this.DrawPixelUnsafe(index, color)
                        index <- index + (direction + width)
                else
                    let (major:int32, minor:int32, majorAdv:int32, minorAdv:int32) =
                        if deltaX > deltaY then
                            // X-Major Line
                            (deltaX, deltaY, direction, width)
                        else
                            // Y-Major Line
                            (deltaY, deltaX, width, direction)

                    let (pixelsPerStep:int32, pixelsPerStepRem:int32) = Math.DivRem(major, minor)

                    let adjustUp:int32 = pixelsPerStepRem * 2
                    let adjustDown:int32 = (minor * 2)

                    let initialPixelStep:int32 = 
                        if (adjustUp = 0) && ((pixelsPerStep &&& 1) = 0) then
                            (pixelsPerStep / 2)
                        else
                            (pixelsPerStep / 2) + 1

                    let mutable errorTerm:int32 =
                        if (pixelsPerStep &&& 1) <> 0 then
                            pixelsPerStepRem - minor
                        else
                            pixelsPerStepRem - (minor * 2)

                    assert (initialPixelStep <> 0)

                    for pixelsDrawn = 0 to (initialPixelStep - 1) do
                        this.DrawPixelUnsafe(index, color)
                        index <- index + majorAdv
                    index <- index + minorAdv

                    assert (minor <> 0)

                    for i = 0 to (minor - 2) do
                        let mutable runLength:int32 = pixelsPerStep
                        errorTerm <- errorTerm + adjustUp

                        if errorTerm > 0 then
                            runLength <- runLength + 1
                            errorTerm <- errorTerm - adjustDown

                        if runLength <> 0 then
                            for pixelsDrawn = 0 to (runLength - 1) do
                                this.DrawPixelUnsafe(index, color)
                                index <- index + majorAdv
                        index <- index + minorAdv

                    for pixelsDrawn = 0 to (initialPixelStep - 1) do
                        this.DrawPixelUnsafe(index, color)
                        index <- index + majorAdv

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
