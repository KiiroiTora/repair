module FSharpCode.Network
open System
open Godot
open WebSocketSharp.Server
open FSharpx.Collections
open Newtonsoft.Json
open FSharpCode.Player

type Lobby() =
    inherit WebSocketBehavior()
    
    static member val playerinputs = PersistentHashMap.empty with get, set 
    
    override this.OnOpen() =
        GD.Print "Someone connected"
        Lobby.playerinputs <- Lobby.playerinputs.Add(this.ID, None)
        ()
    
    override this.OnMessage(e) =
        let e = (System.Text.Encoding.ASCII.GetString(e.RawData))
        
        match  JsonConvert.DeserializeObject<Message>(e) with
        | ClientInputs(inps) ->
            Lobby.playerinputs <- Lobby.playerinputs.Add(this.ID, Some(inps))
            let all_inps = Seq.map snd (Lobby.playerinputs.Iterator())
            
            if (Seq.forall Option.isSome all_inps) then do
                GD.Print "Sending"
                this.broadcast(ServerState(all_inps |> Seq.choose id |>Seq.toList, float32(DateTime.Now.Millisecond)/1000.0f)(*.ToAscii()*))
                Lobby.playerinputs <- Lobby.playerinputs |> PersistentHashMap.map (fun x -> None)
        | _ ->
            GD.Print "Other Message"
            ()
        
    member this.send (msg: Message) =
        this.SendAsync(JsonConvert.SerializeObject(msg), null)
    member this.broadcast (msg: Message) =
        this.Sessions.BroadcastAsync(JsonConvert.SerializeObject(msg), null)
        
and LobbyFs() =
    inherit Node2D()
and ServerFs() =
    inherit Node()
    static member val ws = lazy (
        let ret = new WebSocketServer(8080)
        ret.AddWebSocketService<Lobby>("/lobby")
        
        ret.Start()
        GD.Print("Server started")
        ret
    )
    
    override this._Ready() =
        ServerFs.ws.Value |> ignore

