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
        | MovementDirection(dir) ->
//            Lobby.players.Item(this.ID).mouse_pos <- pos
            this.SendAsync (JsonConvert.SerializeObject(PlayerPositions (List.map (fun (_ , x : PlayerFS) -> x.GlobalPosition) (Lobby.players.Iterator() |> List.ofSeq) ) )(*.ToAscii()*), null)
        | _ ->
            GD.Print "Other Message"
            ()
        
        
        
        
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

