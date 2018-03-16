namespace DemoApplication.Graphics

open DemoApplication.Mathematics
open System.Collections.Generic

type Polygon public (verticeCount:int32, verticeGroupCount:int32, normalCount:int32, normalGroupCount:int32) =
    // Fields
    let vertices:List<Vector3> = new List<Vector3>(verticeCount)
    let verticeGroups:List<int32 array> = new List<int32 array>(verticeGroupCount)
    let modifiedVertices:List<Vector3> = new List<Vector3>(verticeCount)

    let normals:List<Vector3> = new List<Vector3>(normalCount)
    let normalGroups:List<int32 array> = new List<int32 array>(normalGroupCount)
    let modifiedNormals:List<Vector3> = new List<Vector3>(normalCount)

    // Properties
    member public this.Vertices with get() : List<Vector3> = vertices
    member public this.VerticeGroups with get() : List<int32 array> = verticeGroups
    member public this.ModifiedVertices with get() : List<Vector3> = modifiedVertices

    member public this.Normals with get() : List<Vector3> = normals
    member public this.NormalGroups with get() : List<int32 array> = normalGroups
    member public this.ModifiedNormals with get() : List<Vector3> = modifiedNormals

    // Methods
    member public this.Clear() : unit =
        modifiedVertices.Clear()
        modifiedNormals.Clear()

    member public this.Reset() : unit =
        this.Clear()

        modifiedVertices.AddRange(vertices)
        modifiedNormals.AddRange(normals)
