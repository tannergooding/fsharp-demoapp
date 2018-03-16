namespace DemoApplication.Graphics

type IDrawingContext =
    // Properties
    abstract member Handle:nativeint with get

    // Methods
    abstract member DrawBitmap : bitmap:Bitmap -> unit
