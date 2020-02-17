module FSharpCode.ThrowingAxe

open Godot
open Exts

type ThrowingAxeFs() as this=
    inherit Pickup()
    
    let pick_up = new Event<ObjectType>()

    let impact_sound = this.GetNode'<AudioStreamPlayer>("impact_sound")
    let sprite = this.GetNode'<Sprite>("Sprite")
    let area = this.GetNode'<Area2D>("Area2D")

    let flying_curve = ResourceLoader.Load<Curve>("res://Resources/FlyingObjectCurve.tres") 
    
    let rot_speed = lazy(float32 <| GD.RandRange(360.0*2.0, 360.0*5.0))
    let duration = lazy(1.0f)
    let disappearing_sprite = ResourceLoader.Load'<PackedScene>("res://Scenes/DisappearingSprite.tscn")

    let mutable elapsed = 0.0f
    
    member val velocity = Vector2.Zero with get, set
    member val speed = 600.0f with get, set

    member this.can_pick_up () = elapsed >= duration.Value    
    member this.on_pick_up = pick_up.Publish
    override this._Process(delta) =
        elapsed <- elapsed + delta
        this.velocity <- 
            if elapsed >= duration.Value 
            then Vector2.Zero 
            else 
                let coll = this.MoveAndCollide(this.velocity * this.speed * delta * flying_curve.Interpolate(elapsed/duration.Value))
                sprite.Value.Rotate <| Mathf.Deg2Rad(rot_speed.Value) * delta * flying_curve.Interpolate(elapsed/duration.Value)
                if not (coll = null)
                then 
                    impact_sound.Value.Play'()
                    this.velocity.Bounce(coll.Normal)
                else
                    this.velocity
        let s1 = disappearing_sprite.Value.Instance() :?> Sprite
        s1.Texture <- sprite.Value.Texture
        s1.Position <- this.Position
        s1.Rotation <- sprite.Value.Rotation
        s1.ZIndex <- -1
        this.GetParent().AddChild(s1)

        if this.can_pick_up() then do area.Value.SetDeferred("monitorable", true)
    member this._on_Area2D_area_entered(area: Area2D)=
        if area.GetParent().HasMethod("slice") && not (this.can_pick_up()) then do area.GetParent().Call("slice") |> ignore


