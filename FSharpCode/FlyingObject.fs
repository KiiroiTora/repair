module FSharpCode.FlyingObject

open Godot
open Exts

type FlyingObjectFs() =
    inherit KinematicBody2D()
    member val obj_type = ObjectType.H

    let sprite = this.GetNode'<Sprite> "Sprite" // ?
    let area = this.GetNode'<Area2D> "Area2D"

    let type = "LH"
    let flying_curve = ResourceLoader.Load'<Curve>("res://Resources/FlyingObjectCurve.tres")

    let rot_speed = RandRange(360*4, 360*8)  // ?
    let duration = RandRange(360*4, 360*8)  // ?

    signal PickUp(type) // ?

    let mutable velocity = Vector2.Zero // ?
    let mutable elapsed = 0.0f
    let can_pick_up = true 

    let mutable particles = Particles2D // ?

    override this._Ready() =
        velocity <- Vector2(RandRange(-1, 1), RandRange(-1, 1)).Normalized() * RandRange(600, 900)

    override this._Process (delta) =
        elapsed <- elapsed + delta
    	if not (particles = null) then do 
    		GD.Print("Know blood")
    		particles.GlobalPosition <- (this.GetNode'<Sprite> "Sprite/BloodParticles").GlobalPosition // ?
    		particles.GlobalRotation <- (this.GetNode'<Sprite> "Sprite/BloodParticles").GlobalRotation

    	if (elapsed >= duration) then do
    		velocity <- Vector2.ZERO

    	else
    		let coll = MoveAndCollide <| velocity*delta*flying_curve.Interpolate(elapsed/duration)
    		if not (coll = null) do
    			sprite.Rotate(Deg2Rad(rot_speed) * delta * flying_curve.Interpolate(elapsed/duration)) // ?
    	area.SetDeferred("monitorable", velocity == Vector2.ZERO) // ?













