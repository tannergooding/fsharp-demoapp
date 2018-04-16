namespace DemoApplication.Graphics

open DemoApplication.Mathematics
open System
open System.Collections.Generic
open System.IO

type Model public (verticeCount:int32, verticeGroupCount:int32, normalCount:int32, normalGroupCount:int32) =
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

    // Static Methods
    static member public ParseXFile(file:string) : Model =
        let verticeReader = new StreamReader(file + ".vertices")
        let verticeGroupReader = new StreamReader(file + ".verticeGroups")
        let normalReader = new StreamReader(file + ".normals")
        let normalGroupReader = new StreamReader(file + ".normalGroups")

        let verticeCount = Int32.Parse(verticeReader.ReadLine())
        let verticeGroupCount = Int32.Parse(verticeGroupReader.ReadLine())
        let normalCount = Int32.Parse(normalReader.ReadLine())
        let normalGroupCount = Int32.Parse(normalGroupReader.ReadLine())

        let model = new Model(verticeCount, verticeGroupCount, normalCount, normalGroupCount)

        for i = 0 to (verticeCount - 1) do
            let parts = verticeReader.ReadLine().Split(';')
            model.Vertices.Add(new Vector3(Single.Parse(parts.[0]), Single.Parse(parts.[1]), Single.Parse(parts.[2])))

        let group = new List<int32>(4)

        for i = 0 to (verticeGroupCount - 1) do
            let parts = verticeGroupReader.ReadLine().Split(';')
            let groupCount = Int32.Parse(parts.[0])
            let subParts = parts.[1].Split(',')

            for n = 0 to (groupCount - 1) do
                group.Add(Int32.Parse(subParts.[n]))

            model.VerticeGroups.Add(group.ToArray())
            group.Clear()

        for i = 0 to (normalCount - 1) do
            let parts = normalReader.ReadLine().Split(';')
            model.Normals.Add(new Vector3(Single.Parse(parts.[0]), Single.Parse(parts.[1]), Single.Parse(parts.[2])))

        for i = 0 to (normalGroupCount - 1) do
            let parts = normalGroupReader.ReadLine().Split(';')
            let groupCount = Int32.Parse(parts.[0])
            let subParts = parts.[1].Split(',')

            for n = 0 to (groupCount - 1) do
                group.Add(Int32.Parse(subParts.[n]))

            model.NormalGroups.Add(group.ToArray())
            group.Clear()

        model

    // Methods
    member public this.Clear() : unit =
        modifiedVertices.Clear()
        modifiedNormals.Clear()

    member public this.Reset() : unit =
        this.Clear()

        modifiedVertices.AddRange(vertices)
        modifiedNormals.AddRange(normals)
