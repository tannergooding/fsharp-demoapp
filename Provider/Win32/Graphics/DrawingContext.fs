namespace DemoApplication.Provider.Win32.Graphics

open DemoApplication.Graphics
open DemoApplication.Interop

type DrawingContext public (hdd:HDRAWDIB, hdc:HDC) =
    // Fields
    let hdd:HDRAWDIB = hdd
    let hdc:HDC = hdc

    // Properties
    member public this.Handle with get() : nativeint = hdc

    // Methods
    member public this.DrawBitmap(bitmap:Bitmap) : unit =
        if bitmap <> Unchecked.defaultof<Bitmap> then
            let mutable bitmapInfo:BITMAPINFOHEADER = Unchecked.defaultof<BITMAPINFOHEADER>
            bitmapInfo.biSize <- uint32 sizeof<BITMAPINFOHEADER>
            bitmapInfo.biWidth <- bitmap.Width
            bitmapInfo.biHeight <- bitmap.Height
            bitmapInfo.biPlanes <- uint16 bitmap.PlaneCount
            bitmapInfo.biBitCount <- uint16 bitmap.BitsPerPixel
            bitmapInfo.biCompression <- Gdi32.BI_RGB
            bitmapInfo.biSizeImage <- uint32 bitmap.Length

            Msvfw32.DrawDibDraw(hdd, hdc, 0, 0, bitmap.Width, bitmap.Height, &bitmapInfo, bitmap.Handle, 0, 0, bitmap.Width, bitmap.Height, 0u) |> ignore

     // DemoApplication.Graphics.IDrawingContext
    interface IDrawingContext with
        // Properties
        member this.Handle with get() : nativeint = this.Handle

        // Methods
        member this.DrawBitmap(bitmap:Bitmap) : unit = this.DrawBitmap(bitmap)
