module FSharpCode.Network
open Godot
open FSharpCode.Player

type ClientFs() =
    inherit Node2D()
    let mutable last_time = None
    static member val ws = lazy (
        GD.Print "Connecting"
        let ret = new WebSocketClient'("ws://35.214.86.28:8080/lobby")
        ret
    )
    
    member this.locksteppers() = this.GetTree().GetNodesInGroup "lockstep" |> Seq.cast<PlayerFS>
    override this._Ready() =
        ClientFs.ws.Value.OnConnected.Add
            <| fun msg -> GD.Print "Connected"
        ClientFs.ws.Value.OnMessage.Add
            <| fun msg ->
                    GD.Print "Received message"
                    match msg with
                    | ClientInputs(_) -> ()
                    | ServerState (inps, timestamp) ->
                        
                        GD.Print("Received inputs ", inps, timestamp)
                        for (inp, p) in Seq.zip inps (this.locksteppers()) do
                            GD.Print "Setting local inputs"
                            p.inputs <- inp
                            p._Process(timestamp - match last_time with | None -> timestamp | Some(x) -> x)
                            last_time <- Some(timestamp)
                            () 
                        
                        ()
                        
                
        ()
    
    override this._Process(delta) =
        GD.Seed(1UL)
        ClientFs.ws.Value.Poll()
        ClientFs.ws.Value.send <| ClientInputs {
            mouse_pos = this.GetGlobalMousePosition();
            is_charge_pressed = Input.IsActionPressed "throw1";
            is_charge_just_released = Input.IsActionJustReleased "throw1";
            is_run_pressed = Input.IsActionPressed "run1";
        }
        
        
