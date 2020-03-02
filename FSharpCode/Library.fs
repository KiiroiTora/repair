
namespace FSharpCode

open System
open Godot

type ObjectType = LH | RH | RL | LL | H | AXE
module Exts =

    type AudioStreamPlayer with
        member this.Play'() =
            this.PitchScale <- float32 <| GD.RandRange(0.9, 1.1)
            this.Play()


    type ResourceLoader with
        static member Load'<'a when 'a: not struct>(path: string) =
            lazy (ResourceLoader.Load<'a>(path))
    type Node2D with
        member this.XFlipDir (dir : float32) =  
            this.Scale <-
                if dir > 0.0f
                then Vector2(abs (this.Scale.x), abs (this.Scale.y))
                else
                   if dir < 0.0f
                    then Vector2(-abs (this.Scale.x), abs (this.Scale.y))
                    else this.Scale
    type KinematicBody2D with 
      member this.MoveAndCollide' relative_velocity= 
        let coll = this.MoveAndCollide(relative_velocity)
        if isNull coll then None else Some(coll)
    type Godot.Collections.Array with 
        member this.Empty() = this.Count > 0
        member this.PopFront() = 
            let ret = this.Item 0
            this.RemoveAt 0
            ret
        member this.PushFront x = this.Insert(0, x)
    type Pickup() = 
      inherit KinematicBody2D()
      member val obj_type = H with get, set
    type Node with
        member this.GetNode'<'a when 'a :> Node>(path: string) =
            lazy (this.GetNode(new NodePath(path)) :?> 'a)
        member this.as_pickup =
          if this :? Pickup
          then Some(this :?> Pickup)
          else None
          
type Inputs = {mouse_pos: Vector2; is_charge_pressed: bool; is_charge_just_released: bool; is_run_pressed: bool;}
type ServerState = (Inputs list) * DateTime
type Message =
    | ClientInputs of Inputs
    | ServerState of ServerState
