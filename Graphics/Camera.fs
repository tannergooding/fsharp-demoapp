namespace DemoApplication.Graphics

open DemoApplication.Mathematics
open System
open System.Collections.Generic
open System.IO

type Camera =
    // Fields
    val mutable private _cameraToWorld:OrthogonalTransform
    val mutable private _basis:Matrix3x3
    val mutable private _view:Matrix4x4
    val mutable private _projection:Matrix4x4
    val mutable private _viewProjection:Matrix4x4
    val mutable private _previousViewProjection:Matrix4x4
    val mutable private _reprojection:Matrix4x4
    val mutable private _viewSpace:BoundingFrustum
    val mutable private _worldSpace:BoundingFrustum
