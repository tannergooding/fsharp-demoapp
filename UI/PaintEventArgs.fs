namespace DemoApplication.UI

open DemoApplication.Graphics
open System

type PaintEventArgs public (drawingContext:IDrawingContext) =
    inherit EventArgs()

    // Fields
    let drawingContext:IDrawingContext = drawingContext

    // Properties
    member public this.DrawingContext with get() : IDrawingContext = drawingContext
