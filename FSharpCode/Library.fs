namespace FSharpCode

open System.Text
open Godot
open Newtonsoft.Json

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
          
type Message =
    | MovementDirection of Vector2
    | PlayerPositions of Vector2 list

type WebSocketClient'(url : string) as this=
    inherit WebSocketClient()
    
    do
        this.ConnectToUrl url |> ignore
        this.Connect("connection_established", this, "on_connected") |> ignore
        this.Connect("data_received", this, "on_message") |> ignore
    let mutable connected = false    
    let _OnConnected = new Event<_>()
    let _OnMessage   = new Event<Message>()
    member val OnConnected = _OnConnected.Publish
    member val OnMessage = _OnMessage.Publish
    member this.on_connected(protocol: obj[]) =
        connected <- true
        _OnConnected.Trigger()
    member this.on_message() = _OnMessage.Trigger(JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(this.GetPeer(1).GetPacket())))
    member this.send (msg:Message) = if connected then do this.GetPeer(1).PutPacket(JsonConvert.SerializeObject(msg).ToAscii()) |> ignore
        
and ClientFs() =
    inherit Node()
    static member val ws = lazy (
        GD.Print "Connecting"
        let ret = new WebSocketClient'("ws://172.22.8.180:8080/lobby")
        ret
    )
    
    override this._Process(delta) =
        ClientFs.ws.Value.Poll()
        