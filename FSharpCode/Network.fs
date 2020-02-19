module FSharpCode.Network
open Godot
open WebSocketSharp.Server
open FSharpx.Collections
open Newtonsoft.Json
open FSharpCode.Player

type Lobby() =
    inherit WebSocketBehavior()
    
    static member val free_players = System.Collections.Generic.Stack<PlayerFS>() with get, set
    static member val players = PersistentHashMap.empty with get, set
    
    override this.OnOpen() =
        Lobby.players<- Lobby.players.Add(this.ID, Lobby.free_players.Pop())
        GD.Print "Someone connected"
        GD.Print Lobby.players
        ()
    
    override this.OnMessage(e) =
        let e = (System.Text.Encoding.ASCII.GetString(e.RawData))
        
        match  JsonConvert.DeserializeObject<Message>(e) with
        | ClientInputs(inps) ->
            Lobby.players.Item(this.ID).mouse_distance <- inps.distance_to_mouse
            Lobby.players.Item(this.ID).controller_dir <- inps.controller_dir
            
            this.send(ServerState({player_positions = (List.map (fun (_ , x : PlayerFS) -> x.GlobalPosition) (Lobby.players.Iterator() |> List.ofSeq)  )} )(*.ToAscii()*))
        | _ ->
            GD.Print "Other Message"
            ()
        
    member this.send (msg: Message) =
        this.SendAsync(JsonConvert.SerializeObject(msg), null)
        
and LobbyFs() =
    inherit Node2D()
    override this._Ready() =
        Lobby.free_players.Push(this.GetNode(new NodePath("Player2")):?>Player.PlayerFS)
        Lobby.free_players.Push(this.GetNode(new NodePath("Player")):?>Player.PlayerFS)
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

