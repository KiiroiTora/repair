module FSharpCode.Player
open Godot
open Exts
open Godot.Collections
open ThrowingAxe
open FSharpPlus
open System
type PlayerFS() as this =
    inherit KinematicBody2D()

    [<Export>]
    let pid = 1

    let moving_curve = ResourceLoader.Load'<Curve>("res://Resources/WalkCurve.tres")
    let throw_strength = ResourceLoader.Load'<Curve>("res://Resources/AxeThrowCurve.tres")

    let max_throw_duration = 1.0f;

    let anim             = this.GetNode'<AnimationPlayer> "body/anim_body"

    let body             = this.GetNode'<Sprite> "body"
    let head             = this.GetNode'<Sprite> "body/head"
    let l_hand           = this.GetNode'<Sprite> "body/l_hand"
    let l_axe            = this.GetNode'<AnimatedSprite> "body/l_hand/axe"
    let r_hand           = this.GetNode'<Sprite> "body/r_hand"
    let r_axe            = this.GetNode'<AnimatedSprite> "body/r_hand/axe"
    let l_leg            = this.GetNode'<Sprite> "body/l_leg"
    let r_leg            = this.GetNode'<Sprite> "body/r_leg"
    let audio_slice      = this.GetNode'<AudioStreamPlayer> "audio_slice" 
    let audio_throw      = this.GetNode'<AudioStreamPlayer> "audio_throw"
    let aim_center       = this.GetNode'<Position2D> "Position2D"
    let axe_spawn_pos    = this.GetNode'<Position2D> "Position2D/Position2D"
    
    let flying_object_scene    = ResourceLoader.Load'<PackedScene>("res://Scenes/FlyingObject.tscn")
    let throwing_axe_scene     = ResourceLoader.Load'<PackedScene>("res://Scenes/ThrowingAxe.tscn")
    let winscreen              = ResourceLoader.Load'<PackedScene>("res://Scenes/Winner.tscn")
    let particles_tscn         = ResourceLoader.Load'<PackedScene>("res://Scenes/BloodParticles.tscn")
   
    let body_parts = dict [
        H, head;
        LH, l_hand;
        RH, r_hand;
        LL, l_leg;
        RL, r_leg;

    ]

    let mutable limbs = [ LH; LL; RL; RH; H ]

    let max_speed = 300.0f
    let speed = max_speed 
    let mutable last_speed = speed
    let mutable acceleration_sign = 1.0f
    let mutable velocity = Vector2.Zero
    let mutable throw_time = 0.0f
    let mutable has_axe_l = true
    let mutable has_axe_r = true
    
//    let client_state : Inputs = 
    
    let sign x = if x >= 0.0f then 1.0f else -1.0f
    let mouse_distance() = (this.GetGlobalMousePosition() - this.GlobalPosition).Length()
    
    
    
    let has_limb x = List.contains x limbs

    let free_hand() = 
        match (has_limb LH , has_limb RH, has_axe_l, has_axe_r) with
        | (true, _, false, _) -> Some(LH)
        | (_, true, _, false) -> Some(RH)
        | _ -> None

    member val inputs = ({
        mouse_pos = this.GetGlobalMousePosition();
        is_charge_pressed = Input.IsActionPressed "charge1";
        is_charge_just_released = Input.IsActionJustPressed "charge1";
        is_run_pressed = Input.IsActionPressed "run1";
    }) with get, set
    
    member this.controller_dir() = this.GlobalPosition.DirectionTo this.inputs.mouse_pos
    
    override this._Ready() =
        body.Value.Texture <- ResourceLoader.Load("res://Images/Character V.3/P" + pid.ToString() + "/Torso.png")
        head.Value.Texture <- ResourceLoader.Load("res://Images/Character V.3/P" + pid.ToString() + "/Head.png")
        l_leg.Value.Texture <- ResourceLoader.Load("res://Images/Character V.3/P" + pid.ToString() + "/L_Leg.png")
        r_leg.Value.Texture <- ResourceLoader.Load("res://Images/Character V.3/P" + pid.ToString() + "/R_Leg.png")
        this.AddToGroup "lockstep"
        this.SetProcess false
        ()
    
    override this._Process (delta) =
        
        let is_charging = this.inputs.is_charge_pressed && (has_axe_l || has_axe_r)
        
        let is_charging_l = is_charging && has_axe_l
        let is_charging_r = is_charging && has_axe_r && not has_axe_l
        let is_running = this.inputs.is_run_pressed
        let speed = ( if is_running then 2.5f else 1.0f ) *
                    ( if  has_limb LL && has_limb RL
                      then speed
                      else (
                               if has_limb LL || has_limb RL
                               then speed / 2.0f
                               else speed / 4.0f
                           )
                      )
        let speed = speed * (if (mouse_distance() > 50.f) then 1.0f else moving_curve.Value.Interpolate((acceleration_sign * (50.f - mouse_distance())) / 100.f + 0.5f) )
        let acceleration = (speed - last_speed)/delta
        acceleration_sign <- sign acceleration
        last_speed <- speed

//        if (just_pressed "restart") then do this.GetTree().ReloadCurrentScene() |> ignore

        for b_part in body_parts.Keys do body_parts.Item(b_part).Value.Visible <- has_limb b_part

        if this.inputs.is_charge_just_released then do this.throw()

        velocity <- if not is_charging then this.MoveAndSlide <| this.controller_dir() * speed else Vector2.Zero
        anim.Value.PlaybackSpeed <- acceleration_sign * ((4.0f* speed) / (2.5f * max_speed)) 

        throw_time <- if is_charging then Mathf.Clamp(throw_time + delta, 0.0f, max_throw_duration) else 0.0f

        l_axe.Value.Animation <- if not is_charging_l then "default" else "charge"
        l_axe.Value.Frame <- if not is_charging_l then 0 else int ((throw_time / max_throw_duration) * 8.0f)
        l_axe.Value.Scale <- if not is_charging_l then Vector2.One else Vector2.One * ((throw_time / max_throw_duration) / 5.0f) + Vector2.One

        r_axe.Value.Animation <- if not is_charging_r then "default" else "charge"
        r_axe.Value.Frame <- if not is_charging_r then 0 else int ((throw_time / max_throw_duration) * 8.0f)
        r_axe.Value.Scale <- if not is_charging_r then Vector2.One else Vector2.One * ((throw_time / max_throw_duration) / 5.0f) + Vector2.One

        this.GetNode'<Sprite>("shadow").Value.Position <- Vector2(this.GetNode'<Sprite>("shadow").Value.Position.x , if List.contains LL limbs || List.contains RL limbs then 137.669f else 60.0f)
        
        body.Value.XFlipDir <| velocity.x

        anim.Value.Play (if not is_charging then (if velocity.Length() = 0.0f then "idle" else "walk") else ("swing" + (if has_axe_l then "l" else "r")))

        if is_charging_l then do l_hand.Value.LookAt(this.controller_dir().Rotated(Mathf.Deg2Rad(-150.0f)) - l_hand.Value.GlobalPosition)
        if is_charging_r then do r_hand.Value.LookAt(this.controller_dir().Rotated(Mathf.Deg2Rad(-150.0f)) - r_hand.Value.GlobalPosition)

        l_axe.Value.Visible <- has_axe_l
        r_axe.Value.Visible <- has_axe_r

    member this.slice() = 
        if not (limbs.IsEmpty) then do
        
            let removed = limbs.Head
            limbs <- limbs.Tail

            let fl_obj = flying_object_scene.Value.Instance() :?> FSharpCode.FlyingObject.FlyingObjectFs

            audio_slice.Value.Play'()

            if ((removed = LH && has_axe_l) || (removed = RH && has_axe_r)) then do
                let axe = flying_object_scene.Value.Instance() :?> FSharpCode.FlyingObject.FlyingObjectFs
                axe.obj_type <- AXE
                axe.GlobalPosition <- this.GlobalPosition
                (axe.GetNode'<Sprite> "Sprite").Value.Texture <- ResourceLoader.Load("res://Images/Weapon Charge/Axe.png")
                this.GetParent().CallDeferred("add_child", axe)

            has_axe_l <- if removed = LH then false else has_axe_l
            has_axe_r <- if removed = RH then false else has_axe_r

            fl_obj.obj_type <- removed
            fl_obj.GlobalPosition <- this.GlobalPosition

            (fl_obj.GetNode'<Sprite> "Sprite").Value.Texture <- body_parts.Item(removed).Value.Texture
            this.GetParent().CallDeferred("add_child", fl_obj)
            (fl_obj.GetNode'<CPUParticles2D> "Sprite/BloodParticles/BloodParticles").Value.Emitting <- true
            
            if limbs.IsEmpty then do
                this.die()
                
    member this.die() = 
        this.GetNode'<Timer>("timer_win").Value.Start()
        
    member this.throw() =
        if has_axe_l || has_axe_r
        then
            has_axe_r <- if not has_axe_l && has_axe_r then false else has_axe_r
            has_axe_l <- if has_axe_l then false else has_axe_l
            audio_throw.Value.Play'()
            let axe_i = throwing_axe_scene.Value.Instance() :?> ThrowingAxeFs
            aim_center.Value.LookAt(aim_center.Value.GlobalPosition + this.controller_dir())
            axe_i.GlobalPosition <- axe_spawn_pos.Value.GlobalPosition
 
            axe_i.obj_type <- AXE
            axe_i.velocity <- (axe_spawn_pos.Value.GlobalPosition - aim_center.Value.GlobalPosition).Normalized()
            axe_i.speed <- throw_strength.Value.Interpolate(throw_time/max_throw_duration) * 2900.0f + 750.0f 
            axe_i.Scale <- Vector2.One + Vector2.One * (throw_time/max_throw_duration)/5.0f
            axe_i.GetNode'<Sprite>("Sprite").Value.Texture <- ResourceLoader.Load'("res://Images/Weapon Charge/Axe.png").Value
            this.GetParent().AddChild(axe_i)
        else


        ()


    member this._on_area_pickup_area_entered(area: Area2D) =
        monad {
            let! obj = area.GetParent().as_pickup
            let t = obj.obj_type
            GD.Print ("near ", t)
            obj.obj_type <- 
                match t with
                | LH | RH when not (has_limb LH) -> LH
                | LH | RH when not (has_limb RH) -> RH
                | LH | RH -> t
                | LL | RL when not (has_limb LL) -> LL
                | LL | RL when not (has_limb RL) -> RL
                | LL | RL -> t
                | _ -> t
        
        
            if (t <> AXE) && not <| has_limb (t) 
            then do
                limbs <- t :: limbs
                obj.QueueFree()
            else 
                if t = AXE 
                then
                    GD.Print "Near Axe"
                    let! f_hand = free_hand()
                    obj.QueueFree()
                    match f_hand with 
                    | LH -> has_axe_l <- true
                    | RH -> has_axe_r <- true
        } |> ignore
        ()
    member this._on_timer_win_timeout() =
        let wscr = winscreen.Value.Instance()
        let v = (wscr.GetNode<Control> (new NodePath("CanvasLayer/" + (if (pid = 2) then "1" else "2"))))
        v.Visible <- true
        this.GetParent().AddChild(wscr)
        this.GetParent().RemoveChild(this)
        this.CallDeferred("free")        