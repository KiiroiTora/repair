module FSharpCode.FlyingObject

open Godot
open Exts

type FlyingObjectFs() as this=
    inherit KinematicBody2D()

    let sprite = this.GetNode'<Sprite> "Sprite" 
    let area = this.GetNode'<Area2D> "Area2D"

    let flying_curve = ResourceLoader.Load'<Curve>("res://Resources/FlyingObjectCurve.tres")

    let rot_speed = float32 <| GD.RandRange(360.0*4.0, 360.0*8.0) 
    let duration = float32 <| GD.RandRange(360.0*4.0, 360.0*8.0)  

    let mutable velocity = Vector2.Zero 
    let mutable elapsed = 0.0f

    member val obj_type = H with get, set
    override this._Ready() =
        velocity <- Vector2(float32 <| GD.RandRange(-1.0, 1.0), float32 <| GD.RandRange(-1.0, 1.0)).Normalized() * (float32 <| GD.RandRange(600.0, 900.0))

    override this._Process (delta) =
        elapsed <- elapsed + delta

        if (elapsed >= duration) then do
            velocity <- Vector2.Zero

        else
            let coll = this.MoveAndCollide <| velocity*delta*flying_curve.Value.Interpolate(elapsed/duration)
            if not <| isNull coll then do
                sprite.Value.Rotate(Mathf.Deg2Rad(rot_speed) * delta * flying_curve.Value.Interpolate(elapsed/duration)) 
        area.Value.SetDeferred("monitorable", (velocity = Vector2.Zero)) 













