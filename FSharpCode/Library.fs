namespace FSharpCode

open System
open System.Text
open Godot
open Newtonsoft.Json
open FSharp.Data

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
    type Lockstepper =
        abstract member _Lockstep : float32 -> unit
    type Node with
        member this.GetNode'<'a when 'a :> Node>(path: string) =
            lazy (this.GetNode(new NodePath(path)) :?> 'a)
        member this.as_pickup =
          if this :? Pickup
          then Some(this :?> Pickup)
          else None
module Seq =
    let fromGDArray (xs:Collections.Array) = 
        seq {
            for x in xs do
                yield x
            } 






type Inputs = {mouse_pos: Vector2; is_charge_pressed: bool; is_charge_just_released: bool; is_run_pressed: bool;}
type ServerState = (Inputs list) * DateTime
type Message =
    | ClientInputs of Inputs
    | ServerState of ServerState

type WebSocketClient'(url : string) as this=
    inherit WebSocketClient()
    
    do
//        this.ConnectToUrl url |> ignore
        this.Connect("connection_established", this, "on_connected") |> ignore
        this.Connect("data_received", this, "on_message") |> ignore
        this.Connect("connection_closed", this, "on_connection_closed") |> ignore
        this.Connect("connection_error", this, "on_disconnected") |> ignore
        
    
    let _OnConnected          = new Event<_>()
    let _OnMessage            = new Event<Message>()
    let _OnConnectionClosed   = new Event<_>()
    let _OnDisconnected       = new Event<_>()
    
    member val connected = false with set, get    
    member val OnConnected           = _OnConnected.Publish
    member val OnMessage             = _OnMessage.Publish
    member val OnConnectionClosed    = _OnConnectionClosed.Publish
    member val OnDisconnected        = _OnDisconnected.Publish
    
    member this.on_connected(protocol: obj[]) =
        this.connected <- true
        _OnConnected.Trigger()
    member this.on_message() = _OnMessage.Trigger(JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(this.GetPeer(1).GetPacket())))
    member this.on_connection_closed(was_clean_close: obj[]) =
        this.connected <- false
        _OnConnectionClosed.Trigger()
    member this.on_disconnected() =
        this.connected <- false
        _OnDisconnected.Trigger()
    
    member this.connect () = this.ConnectToUrl url |> ignore
    member this.send (msg:Message) = if this.connected then do this.GetPeer(1).PutPacket(JsonConvert.SerializeObject(msg).ToAscii()) |> ignore

