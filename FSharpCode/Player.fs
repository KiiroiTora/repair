module FSharpCode.Player
open Godot
open Exts
open Godot.Collections
open ThrowingAxe

type PlayerFS() as this =
    inherit KinematicBody2D()

    [<Export>]
    let pid = 1

    let throw_strength = ResourceLoader.Load'<Curve>("res://Resources/AxeThrowCurve.tres")

    let max_throw_duration = 2.0f;

    let anim             = this.GetNode'<AnimationPlayer> "body/anim_body"

    let body             = this.GetNode'<Sprite> "body"
    let head             = this.GetNode'<Sprite> "body/head"
    let l_hand           = this.GetNode'<Sprite> "body/l_hand"
    let l_axe            = this.GetNode'<AnimatedSprite> "body/l_hand/axe"
    let r_hand           = this.GetNode'<Sprite> "body/r_hand"
    let r_axe            = this.GetNode'<AnimatedSprite> "body/r_hand/axe"
    let l_leg            = this.GetNode'<Sprite> "body/l_leg"
    let r_leg            = this.GetNode'<Sprite> "body/r_leg"
    let audio_throw      = this.GetNode'<AudioStreamPlayer> "audio_throw"
    let aim_center       = this.GetNode'<Position2D> "Position2D"
    let axe_spawn_pos    = this.GetNode'<Position2D> "Position2D/Position2D"
    
    let flying_object_scene    = ResourceLoader.Load'<PackedScene>("res://Scenes/FlyingObject.tscn")
    let throwing_axe_scene     = ResourceLoader.Load'<PackedScene>("res://Scenes/ThrowingAxe.tscn")
    let winscreen              = ResourceLoader.Load'<PackedScene>("res://Scenes/Winner.tscn")
    let particles_tscn         = ResourceLoader.Load'<PackedScene>("res://Scenes/BloodParticles.tscn")
   
    let body_parts = dict [
        "H", head;
        "LH", l_hand;
        "RH", r_hand;
        "LL", l_leg;
        "RL", r_leg;

    ]

    let limbs: Array = new Array([ "LH"; "LL"; "RL"; "RH"; "H" ])

    let speed = 350.0f

    let mutable velocity = Vector2.Zero
    let mutable throw_time = 0.0f
    let mutable has_axe_l = true
    let mutable has_axe_r = true
    
    
    let just_pressed s       = Input.IsActionJustPressed s
    let pressed s            = Input.IsActionPressed(s + pid.ToString())
    let just_released s      = Input.IsActionJustReleased(s + pid.ToString())
    let action_strength s    = Input.GetActionStrength(s + pid.ToString())
    let controller_dir()     = Vector2(action_strength "right" - action_strength "left", action_strength "down" - action_strength "up")
    
    override this._Ready() =
        body.Value.Texture <- ResourceLoader.Load("res://Images/Character V.3/P" + pid.ToString() + "/Torso.png")
        head.Value.Texture <- ResourceLoader.Load("res://Images/Character V.3/P" + pid.ToString() + "/Head.png")
        l_leg.Value.Texture <- ResourceLoader.Load("res://Images/Character V.3/P" + pid.ToString() + "/L_Leg.png")
        r_leg.Value.Texture <- ResourceLoader.Load("res://Images/Character V.3/P" + pid.ToString() + "/R_Leg.png")
        GD.Randomize()
        ()

    override this._Process (delta) =


        let is_charging = pressed "throw" && (has_axe_l || has_axe_r)
        let is_charging_l = is_charging && has_axe_l
        let is_charging_r = is_charging && has_axe_r && not has_axe_l
        let is_running = pressed "run"
        let speed = ( if is_running then 2.5f else 1.0f ) *
                    ( if limbs.Contains("LL") && limbs.Contains("RL")
                      then speed
                      else (
                               if limbs.Contains("LL") || limbs.Contains("RL")
                               then speed / 2.0f
                               else speed / 4.0f
                           )
                      )

        if (just_pressed "restart") then do this.GetTree().ReloadCurrentScene() |> ignore

        for b_part in body_parts.Keys do body_parts.Item(b_part).Value.Visible <- limbs.Contains b_part

        if just_released "throw" then do this.throw()

        velocity <- if not is_charging then this.MoveAndSlide <| controller_dir() * speed else Vector2.Zero

        anim.Value.PlaybackSpeed <- if is_running then 4.0f else 2.0f

        throw_time <- if is_charging then Mathf.Clamp(throw_time + delta, 0.0f, max_throw_duration) else 0.0f

        l_axe.Value.Animation <- if not is_charging_l then "default" else "charge"
        l_axe.Value.Frame <- if not is_charging_l then 0 else int ((throw_time / max_throw_duration) * 8.0f)
        l_axe.Value.Scale <- if not is_charging_l then Vector2.One else Vector2.One * ((throw_time / max_throw_duration) / 5.0f) + Vector2.One

        r_axe.Value.Animation <- if not is_charging_r then "default" else "charge"
        r_axe.Value.Frame <- if not is_charging_r then 0 else int ((throw_time / max_throw_duration) * 8.0f)
        r_axe.Value.Scale <- if not is_charging_r then Vector2.One else Vector2.One * ((throw_time / max_throw_duration) / 5.0f) + Vector2.One

        body.Value.XFlipDir <| controller_dir().x

        anim.Value.Play (if not is_charging then (if velocity.Length() = 0.0f then "idle" else "walk") else ("swing" + (if has_axe_l then "l" else "r")))

        if is_charging_l then do l_hand.Value.LookAt(controller_dir().Rotated(Mathf.Deg2Rad(-150.0f)) - l_hand.Value.GlobalPosition)
        if is_charging_r then do r_hand.Value.LookAt(controller_dir().Rotated(Mathf.Deg2Rad(-150.0f)) - r_hand.Value.GlobalPosition)

    member this.throw() =
        if has_axe_l || has_axe_r
        then
            has_axe_r <- if not has_axe_l && has_axe_r then false else has_axe_r
            has_axe_l <- if has_axe_l then false else has_axe_l
            audio_throw.Value.Play'()
            let axe_i = throwing_axe_scene.Value.Instance() :?> ThrowingAxeFs
            aim_center.Value.LookAt(aim_center.Value.GlobalPosition + controller_dir())
            axe_i.GlobalPosition <- axe_spawn_pos.Value.GlobalPosition
 
            axe_i.velocity <- (axe_spawn_pos.Value.GlobalPosition - aim_center.Value.GlobalPosition).Normalized()
            axe_i.speed <- throw_strength.Value.Interpolate(throw_time/max_throw_duration) * 2700.0f + 550.0f 
            axe_i.Scale <- Vector2.One + Vector2.One * (throw_time/max_throw_duration)/5.0f
            axe_i.GetNode'<Sprite>("Sprite").Value.Texture <- ResourceLoader.Load'("res://Images/Weapon Charge/Axe.png").Value
            this.GetParent().AddChild(axe_i)
        else


        ()

    member this._on_area_pickup_area_entered(area: Area) =
//       let obj = area.GetParent() :?> FSharpCode.FlyingObject.FlyingObjectFs
//       obj.obj_type
        ()
