module FSharpCode.Network
open FSharpCode.Exts
open System.Threading
open Godot
open FSharpCode.Player
open FSharpx.Collections
open FSharpPlus

type ClientFs() as this =
    inherit Node2D()
    let delay_latency = 0.5f
    let mutable delay_latency_timer = 0.0f
    let mutable last_time = None
    let mutable inputs_queue = Queue.empty
    
    
    static member val ws = lazy (
        GD.Print "Connecting"
        let ret = new WebSocketClient'("wss://35.214.86.28:8080/lobby")
//        let ret = new WebSocketClient'("ws://35.214.86.28:8080/lobby")
        ret
    )
    
    member this.locksteppers() = this.GetTree().GetNodesInGroup "lockstep" |> Seq.cast<Lockstepper> |> List.ofSeq
//    member this.players() = this.locksteppers() |> Seq.filter (fun x -> x :? PlayerFS) |> Seq.cast<PlayerFS>
    
    override this._Ready() =
//        GD.Seed(1UL)
        ClientFs.ws.Value.OnConnected.Add
            <| fun _ -> GD.Print "Connected"
        ClientFs.ws.Value.OnMessage.Add
            <| fun msg ->
                    match msg with
                    | ClientInputs(_) -> ()
                    | ServerState (inps, timestamp) ->
                        GD.Print "message received"
                        inputs_queue <- inputs_queue |> Queue.conj (ServerState(inps, timestamp))
                        
        ClientFs.ws.Value.OnConnectionClosed.Add <| fun _ -> this.GetTree().ReloadCurrentScene() |> ignore
        ClientFs.ws.Value.OnDisconnected.Add <| fun _ -> this.GetTree().ReloadCurrentScene() |> ignore
        
        ()
    
    override this._Process(delta) =
        delay_latency_timer <- if ClientFs.ws.Value.connected then delay_latency_timer + delta else 0.0f
        ClientFs.ws.Value.Poll()
        let to_send = (ClientInputs {
                        mouse_pos = this.GetGlobalMousePosition();
                        is_charge_pressed = Input.IsActionPressed "throw1";
                        is_charge_just_released = Input.IsActionJustReleased "throw1";
                        is_run_pressed = Input.IsActionPressed "run1";
                    })
//        GD.Print inputs_queue.Length
        ClientFs.ws.Value.send <| to_send
        if delay_latency_timer > delay_latency then do
            monad{
                let! ServerState(inps, timestamp) = inputs_queue |> Queue.tryHead
                inputs_queue <- inputs_queue.Tail
                let ls = this.locksteppers()
                let ps = ls |> Seq.filter (fun x -> x :? PlayerFS) |> Seq.cast<PlayerFS> |> List.ofSeq
                let dt = timestamp - match last_time with | None -> timestamp | Some(x) -> x
                let dt = float32(dt.TotalMilliseconds)/1000.0f
                    
                GD.Print("Delta sent: ", dt)
                for (inp, p) in List.zip inps (ps) do
                    p.inputs <- inp
                for l in ls do
                    l._Lockstep(dt)
                last_time <- Some(timestamp)
                
            } |> ignore
type Arena1Fs() =
    inherit Node2D()
    override this._Ready() = ClientFs.ws.Value.connect()
     
        
        
        
        
        
        
