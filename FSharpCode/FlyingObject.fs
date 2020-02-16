module FSharpCode.FlyingObject

open Godot
open Exts

type FlyingObjectFs() =
    inherit KinematicBody2D()
    member val obj_type = ObjectType.H

    let sprite = this.GetNode'<Sprite> "Sprite" 
    let area = this.GetNode'<Area2D> "Area2D"

    let type = "LH"
    let flying_curve = ResourceLoader.Load'<Curve>("res://Resources/FlyingObjectCurve.tres")

    let rot_speed = GD.RandRange(360.0*4.0, 360.0*8.0) 
    let duration = GD.RandRange(360.0*4.0, 360.0*8.0)  

    [<Signal>]
    PickUp(type) 

    let mutable velocity = Vector2.Zero 
    let mutable elapsed = 0.0f
    let can_pick_up = true 

    let mutable particles = Particles2D 

    override this._Ready() =
        velocity <- Vector2(GD.RandRange(-1.0, 1.0), GD.RandRange(-1.0, 1.0)).Normalized() * GD.RandRange(600.0, 900.0)

    override this._Process (delta) =
        elapsed <- elapsed + delta
    	if not (particles = null) then do 
    		GD.Print("Know blood")
    		particles.GlobalPosition <- (this.GetNode'<Sprite> "Sprite/BloodParticles").GlobalPosition 
    		particles.GlobalRotation <- (this.GetNode'<Sprite> "Sprite/BloodParticles").GlobalRotation

    	if (elapsed >= duration) then do
    		velocity <- Vector2.ZERO

    	else
    		let coll = MoveAndCollide <| velocity*delta*flying_curve.Interpolate(elapsed/duration)
    		if not (coll = null) do
    			sprite.Rotate(Deg2Rad(rot_speed) * delta * flying_curve.Interpolate(elapsed/duration)) 
    	area.SetDeferred("monitorable", velocity == Vector2.ZERO) 













