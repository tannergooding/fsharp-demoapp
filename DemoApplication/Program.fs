// Learn more about F# at http://fsharp.org

open DemoApplication
open DemoApplication.Graphics
open DemoApplication.Mathematics
open DemoApplication.Provider.Win32.Threading
open DemoApplication.Provider.Win32.UI
open System

type Program public () =
    // Constants
    [<Literal>]
    let BufferCount:int32 = 2

    [<Literal>]
    let ConsoleHeaderLineCount:int32 = 9

    [<Literal>]
    let BackgroundColor:uint32 = 0xFF6495EDu // Cornflower Blue

    // Fields
    let dispatchManager:DispatchManager = new DispatchManager()
    let dispatcher:Dispatcher = dispatchManager.DispatcherForCurrentThread :?> Dispatcher
    let windowManager:WindowManager = new WindowManager(dispatchManager)
    let window:Window = windowManager.CreateWindow() :?> Window

    let buffers:Bitmap array = Array.zeroCreate BufferCount

    let mutable displayBufferIndex:int32 = 0
    let mutable renderBufferIndex:int32 = 0

    let mutable minFps:int32 = 0x7FFFFFFF
    let mutable fps:int32 = 0
    let mutable maxFps:int32 = 0x80000000
    let mutable totalFrames:int64 = 0L

    let mutable totalUptime:TimeSpan = TimeSpan.Zero
    let mutable lastHeaderUpdate:TimeSpan = TimeSpan.Zero

    let mutable rotation:Vector3 = new Vector3(75.0f, 0.0f, 0.0f)
    let mutable rotationSpeed:Vector3 = new Vector3(0.0000f, 15.0000f, 0.0000f)
    let mutable scale:Vector3 = new Vector3(500.0f)
    let mutable translation:Vector3 = new Vector3(0.0f)

    let mutable isTriangles:bool = false
    let mutable isRotating:bool = true
    let mutable isCulling:bool = true

    let cube:Model =
        let primitive:Model = new Model(8, 6, 6, 6)

        primitive.Vertices.Add(new Vector3(-1.000000f, -1.000000f, -1.000000f))
        primitive.Vertices.Add(new Vector3(-1.000000f,  1.000000f, -1.000000f))
        primitive.Vertices.Add(new Vector3( 1.000000f,  1.000000f, -1.000000f))
        primitive.Vertices.Add(new Vector3( 1.000000f, -1.000000f, -1.000000f))
        primitive.Vertices.Add(new Vector3(-1.000000f, -1.000000f,  1.000000f))
        primitive.Vertices.Add(new Vector3(-1.000000f,  1.000000f,  1.000000f))
        primitive.Vertices.Add(new Vector3( 1.000000f,  1.000000f,  1.000000f))
        primitive.Vertices.Add(new Vector3( 1.000000f, -1.000000f,  1.000000f))

        primitive.VerticeGroups.Add([| 0; 1; 5; 4 |])
        primitive.VerticeGroups.Add([| 1; 2; 6; 5 |])
        primitive.VerticeGroups.Add([| 2; 3; 7; 6 |])
        primitive.VerticeGroups.Add([| 3; 0; 4; 7 |])
        primitive.VerticeGroups.Add([| 3; 2; 1; 0 |])
        primitive.VerticeGroups.Add([| 4; 5; 6; 7 |])

        primitive.Normals.Add(new Vector3(-1.000000f,  0.000000f,  0.000000f))
        primitive.Normals.Add(new Vector3( 0.000000f,  1.000000f, -0.000000f))
        primitive.Normals.Add(new Vector3( 1.000000f,  0.000000f, -0.000000f))
        primitive.Normals.Add(new Vector3( 0.000000f, -1.000000f,  0.000000f))
        primitive.Normals.Add(new Vector3(-0.000000f,  0.000000f, -1.000000f))
        primitive.Normals.Add(new Vector3(-0.000000f,  0.000000f,  1.000000f))

        primitive.NormalGroups.Add([| 0; 0; 0; 0 |])
        primitive.NormalGroups.Add([| 1; 1; 1; 1 |])
        primitive.NormalGroups.Add([| 2; 2; 2; 2 |])
        primitive.NormalGroups.Add([| 3; 3; 3; 3 |])
        primitive.NormalGroups.Add([| 4; 4; 4; 4 |])
        primitive.NormalGroups.Add([| 5; 5; 5; 5 |])

        primitive

    let mutable activePrimitive:Model =
        try
            Model.ParseXFile("test")
        with
        | _ -> cube

    // Methods
    member public this.Run() : unit =
        Console.CursorTop <- ConsoleHeaderLineCount

        let mutable exitRequested = false
        dispatcher.ExitRequested.Add(fun unit -> exitRequested <- true)

        window.Show()
        window.Paint.Add(fun eventArgs -> 
            eventArgs.DrawingContext.DrawBitmap(buffers.[displayBufferIndex]))

        let mutable previousTimestamp:Timestamp = dispatchManager.CurrentTimestamp

        dispatcher.DispatchPending()

        while not exitRequested do
            let timestamp = dispatchManager.CurrentTimestamp
            this.Idle(timestamp - previousTimestamp)

            previousTimestamp <- timestamp
            dispatcher.DispatchPending()

    member internal this.CameraToScreen(polygon:Model) : unit =
        ()

    member internal this.Idle(delta:TimeSpan) : unit =
        this.Update(delta)
        this.Render()
        this.Present()

    member internal this.ObjectToWorld(polygon:Model) : unit =
        this.RotateObject(polygon)
        this.ScaleObject(polygon)
        this.TranslateObject(polygon)

    member internal this.Present() : unit =
        displayBufferIndex <- renderBufferIndex
        renderBufferIndex <- renderBufferIndex + 1

        if renderBufferIndex = BufferCount then
            renderBufferIndex <- 0

        window.Redraw()
        fps <- fps + 1
        totalFrames <- totalFrames + 1L

    member internal this.Render() : unit =
        this.RenderConsoleInfo()
        this.RenderBuffer()

    member internal this.RenderBuffer() : unit =
        let renderBuffer:Bitmap = buffers.[renderBufferIndex]
        renderBuffer.Clear(BackgroundColor)
        renderBuffer.DrawPolygon(activePrimitive, isTriangles, isCulling)

    member internal this.RenderConsoleInfo() : unit =
        if lastHeaderUpdate.Ticks >= TimeSpan.TicksPerSecond then
            let consoleLeft:int32 = Console.CursorLeft
            Console.CursorLeft <- 0

            let consoleTop:int32 = Console.CursorTop
            Console.CursorTop <- 0

            minFps <- min fps minFps
            maxFps <- max fps maxFps

            Console.WriteLine("========================================")
            Console.WriteLine("{0} FPS", fps)
            Console.WriteLine("{0:F3} Min FPS", minFps)
            Console.WriteLine("{0:F3} Avg FPS", (float32 totalFrames / (float32 totalUptime.Ticks / float32 TimeSpan.TicksPerSecond)))
            Console.WriteLine("{0:F3} Max FPS", maxFps)
            Console.WriteLine()
            Console.WriteLine("{0}x{1} Resolution", window.Bounds.Size.Width, window.Bounds.Size.Height)
            Console.WriteLine("========================================")

            Console.CursorLeft <- consoleLeft
            Console.CursorTop <- consoleTop

            lastHeaderUpdate <- TimeSpan.Zero
            fps <- 0

    member internal this.RotateObject(polygon:Model) : unit =
        let deg2rad:float32 = MathF.PI / 180.0f

        let rotX:float32 = (rotation.X * deg2rad)
        let rotY:float32 = (rotation.Y * deg2rad)
        let rotZ:float32 = (rotation.Z * deg2rad)

        let sinX = MathF.Sin(rotX)
        let cosX = if MathF.Abs(sinX) = 1.0f then
                       0.0f
                   else
                       MathF.Cos(rotX)
        let mX = new Matrix3x3(Vector3.UnitX, new Vector3(0.0f, cosX, -sinX), new Vector3(0.0f, sinX, cosX))

        let sinY = MathF.Sin(rotY)
        let cosY = if MathF.Abs(sinY) = 1.0f then
                       0.0f
                   else
                       MathF.Cos(rotY)
        let mY = new Matrix3x3(new Vector3(cosY, 0.0f, sinY), Vector3.UnitY, new Vector3(-sinY, 0.0f, cosY))

        let sinZ = MathF.Sin(rotZ)
        let cosZ = if MathF.Abs(sinZ) = 1.0f then
                       0.0f
                   else
                       MathF.Cos(rotZ)
        let mZ = new Matrix3x3(new Vector3(cosZ, -sinZ, 0.0f), new Vector3(sinZ, cosZ, 0.0f), Vector3.UnitZ)

        let mR:Matrix3x3 = ((mX * mY) * mZ)

        for i = 0 to (polygon.Vertices.Count - 1) do
            polygon.ModifiedVertices.[i] <- (polygon.ModifiedVertices.[i] * mR)

        for i = 0 to (polygon.Normals.Count - 1) do
            polygon.ModifiedNormals.[i] <- (polygon.ModifiedNormals.[i] * mR)

    member internal this.ScaleObject(polygon:Model) : unit =
        let m:Matrix3x3 = new Matrix3x3(new Vector3(scale.X, 0.0f, 0.0f),
                                        new Vector3(0.0f, scale.Y, 0.0f),
                                        new Vector3(0.0f, 0.0f, scale.Z))

        for i = 0 to (polygon.Vertices.Count - 1) do
            polygon.ModifiedVertices.[i] <- (polygon.ModifiedVertices.[i] * m)

    member internal this.TranslateObject(polygon:Model) : unit =
        for i = 0 to (polygon.Vertices.Count - 1) do
            polygon.ModifiedVertices.[i] <- (polygon.ModifiedVertices.[i] + translation)

    member internal this.Update(delta:TimeSpan) : unit =
        activePrimitive.Reset()
        totalUptime <- totalUptime + delta
        lastHeaderUpdate <- lastHeaderUpdate + delta

        this.UpdateBuffers()
        this.UpdateRotation(delta)

        this.ObjectToWorld(activePrimitive)
        this.WorldToCamera(activePrimitive)
        this.CameraToScreen(activePrimitive)

    member internal this.UpdateBuffers() : unit =
        let width = window.Bounds.Size.Width
        let height = window.Bounds.Size.Height

        translation <- new Vector3((width / 2.0f), (float32 height / 2.0f), 0.0f)
        let renderBuffer:Bitmap = buffers.[renderBufferIndex]

        if renderBuffer = Unchecked.defaultof<Bitmap> then
            buffers.[renderBufferIndex] <- new Bitmap(int32 width, int32 height)
        else
            buffers.[renderBufferIndex].Resize(int32 width, int32 height)

    member internal this.UpdateRotation(delta:TimeSpan) : unit =
        if isRotating then
            rotation <- (rotation + (rotationSpeed * (float32 delta.Ticks / float32 TimeSpan.TicksPerSecond)))

            if rotation.X >= 360.0f then
                rotation <- rotation.WithX(rotation.X - 360.0f)

            if rotation.Y >= 360.0f then
                rotation <- rotation.WithY(rotation.Y - 360.0f)

            if rotation.Z >= 360.0f then
                rotation <- rotation.WithZ(rotation.Z - 360.0f)

    member internal this.WorldToCamera(polygon:Model) : unit =
        ()

[<EntryPoint>]
let main argv =
    let program:Program = new Program()
    program.Run()
    0
