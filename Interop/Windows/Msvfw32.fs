namespace DemoApplication.Interop

open System.Runtime.InteropServices

module Msvfw32 =
    // External Methods
    [<DllImport("Msvfw32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "DrawDibClose", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern BOOL DrawDibClose(
        [<In>] HDRAWDIB hdd
    )

    [<DllImport("Msvfw32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "DrawDibDraw", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern BOOL DrawDibDraw(
        [<In>] HDRAWDIB hdd,
        [<In>] HDC hdc,
        [<In>] int32 xDst,
        [<In>] int32 yDst,
        [<In>] int32 dxDst,
        [<In>] int32 dyDst,
        [<In>] BITMAPINFOHEADER& lpvi,
        [<In; Optional>] LPVOID lpBits,
        [<In>] int32 xSrc,
        [<In>] int32 ySrc,
        [<In>] int32 dxSrc,
        [<In>] int32 dySrc,
        [<In>] UINT wFlags
    )

    [<DllImport("Msvfw32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "DrawDibOpen", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern HDRAWDIB DrawDibOpen()
