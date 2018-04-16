namespace DemoApplication.Graphics

open DemoApplication.Mathematics
open System
open System.Collections.Generic
open System.IO

type PerspectiveCamera public () as this =
    // Fields
    let mutable _cameraToWorld:OrthogonalTransform = OrthogonalTransform.Identity
    let mutable _basis:Matrix3x3 = Matrix3x3.Identity
    let mutable _view:Matrix4x4 = Matrix4x4.Identity
    let mutable _projection:Matrix4x4 = Matrix4x4.Identity
    let mutable _viewProjection:Matrix4x4 = Matrix4x4.Identity
    let mutable _previousViewProjection:Matrix4x4 = Matrix4x4.Identity
    let mutable _reprojection:Matrix4x4 = Matrix4x4.Identity
    let mutable _viewSpace:BoundingFrustum = new BoundingFrustum()
    let mutable _worldSpace:BoundingFrustum = new BoundingFrustum()

    let mutable _fieldOfView:float32 = MathF.PI / 4.0f
    let mutable _aspectRatio:float32 = 9.0f / 16.0f
    let mutable _nearClip:float32 = 1.0f
    let mutable _farClip:float32 = 1000.0f
    let mutable _reverseZ:bool = true

    do this.SetPerspective(MathF.PI / 4.0f, 9.0f / 16.0f, 1.0f, 1000.0f)

    // Properties
    member public this.AspectRatio with get() = _aspectRatio

    member public this.ClearDepth with get() = if _reverseZ then 0.0f else 1.0f

    member public this.FarClip with get() = _farClip

    member public this.FieldOfView with get() = _fieldOfView

    member public this.Forward with get() : Vector3 = -_basis.Z

    member public this.NearClip with get() = _nearClip

    member public this.Position with get() : Vector3 = _cameraToWorld.Translation

    member public this.Projection with get() : Matrix4x4 = _projection

    member public this.Reprojection with get() : Matrix4x4 = _reprojection

    member public this.Right with get() : Vector3 = _basis.X

    member public this.Rotation with get() : Quaternion = _cameraToWorld.Rotation

    member public this.Up with get() : Vector3 = _basis.Y

    member public this.View with get() : Matrix4x4 = _view

    member public this.ViewProjection with get() : Matrix4x4 = _viewProjection

    member public this.ViewSpace with get() : BoundingFrustum = _viewSpace

    member public this.WorldSpace with get() : BoundingFrustum = _worldSpace

    // Methods
    member public this.SetAspectRatio(aspectRatio:float32) =
        _aspectRatio <- aspectRatio

    member public this.SetClip(nearClip:float32, farClip:float32) =
        _nearClip <- nearClip
        _farClip <- farClip

    member public this.SetEyeAtUp(eye:Vector3, at:Vector3, up:Vector3) =
        this.SetLookDirection((at - eye), up)
        this.SetPosition(eye)

    member public this.SetFieldOfView(fieldOfView:float32) =
        _fieldOfView <- fieldOfView

    member public this.SetLookDirection(forward:Vector3, up:Vector3) =
        let forwardLengthSq = forward.LengthSquared
        let forward = if forwardLengthSq < 0.000001f then
                          -Vector3.UnitZ
                      else
                          forward / MathF.Sqrt(forwardLengthSq)

        let mutable right = Vector3.CrossProduct(forward, up)
        let rightLengthSq = right.LengthSquared
        right <- if rightLengthSq < 0.000001f then
                     forward.Transform(new Quaternion(Vector3.UnitY, -MathF.PI / 2.0f))
                 else
                    right / MathF.Sqrt(rightLengthSq)

        let up = Vector3.CrossProduct(right, forward)

        _basis <- new Matrix3x3(right, up, -forward)
        _cameraToWorld <- _cameraToWorld.WithRotation(Quaternion.CreateFrom(_basis))

    member public this.SetPerspective(fieldOfView:float32, aspectRatio:float32, nearClip:float32, farClip:float32) =
        _fieldOfView <- fieldOfView
        _aspectRatio <- aspectRatio
        _nearClip <- nearClip
        _farClip <- farClip

        this.UpdateProjection()

        _previousViewProjection <- _viewProjection

    member public this.SetPosition(position:Vector3) =
        _cameraToWorld <- _cameraToWorld.WithTranslation(position)

    member public this.SetProjection(projection:Matrix4x4) =
        _projection <- projection

    member public this.SetReverseZ(reverseZ:bool) =
        _reverseZ <- reverseZ

    member public this.SetRotation(rotation:Quaternion) =
        _cameraToWorld <- _cameraToWorld.WithRotation(rotation.Normalize())
        _basis <- Matrix3x3.CreateFrom(_cameraToWorld.Rotation)

    member public this.Update() =
        _previousViewProjection <- _viewProjection

        _view <- Matrix4x4.CreateFrom(_cameraToWorld.Invert())
        _viewProjection <- _projection * _view
        _reprojection <- _previousViewProjection * _viewProjection.Invert()

        _viewSpace <- BoundingFrustum.CreateFrom(_projection)
        _worldSpace <- _viewSpace.Transform(_cameraToWorld)

    member private this.UpdateProjection() =
        let y = 1.0f / MathF.Tan(_fieldOfView * 0.5f)
        let x = y * _aspectRatio

        let (q1, q2) = if _reverseZ then
                           let temp = _nearClip / (_farClip - _nearClip)
                           (temp, temp * _farClip)
                       else
                           let temp = _farClip / (_nearClip - _farClip)
                           (temp, temp * _nearClip)

        let projection = new Matrix4x4(new Vector4(x, 0.0f, 0.0f, 0.0f),
                                       new Vector4(0.0f, y, 0.0f, 0.0f),
                                       new Vector4(0.0f, 0.0f, q1, -1.0f),
                                       new Vector4(0.0f, 0.0f, q2, 0.0f))

        this.SetProjection(projection)
