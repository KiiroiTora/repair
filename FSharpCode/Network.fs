module FSharpCode.Network
open System
open System.Security.Cryptography.X509Certificates

open Godot
open WebSocketSharp.Server
open FSharpx.Collections
open Newtonsoft.Json
open FSharpCode.Player
open FSharpPlus
open FSharpx.Collections
type PersistentHashMap() =
    static member containsKey' key hm = try hm |> PersistentHashMap.find key |> fun x -> Option.isSome x || Option.isNone x with _ -> false
type Option<'a> with
    static member to_string x = if Option.isSome x then x.ToString() else "None"
type Lobby() =
    inherit WebSocketBehavior()
    
    let max_in_arena = 2
    static member val queue = [] with get, set
//    static member val playerinputs = PersistentHashMap.empty with get, set 
    static member val arenas : PersistentHashMap<string, Inputs option> list = [] with get, set
       
    override this.OnOpen() =
        Lobby.queue <- List.cons this.ID Lobby.queue
        if List.length Lobby.queue = 2 then do
            GD.Print "Enough players in queue adding an arena"
            Lobby.arenas <- List.cons (PersistentHashMap.ofSeq (Lobby.queue |> List.map (fun x -> (x, None)))) Lobby.arenas
            Lobby.queue <- []
        GD.Print ("Someone connected. Queue: ", Lobby.queue, " and arenas: ", Lobby.arenas)
//            Lobby.playerinputs <- Lobby.playerinputs.Add(this.ID, None)
        ()
    
    override this.OnMessage(e) =
        let e = (System.Text.Encoding.ASCII.GetString(e.RawData))
        
        match  JsonConvert.DeserializeObject<Message>(e) with
        | ClientInputs(inps) ->
            Lobby.arenas <- Lobby.arenas |> List.map (fun x -> if PersistentHashMap.containsKey' this.ID x then PersistentHashMap.add this.ID (Some(inps)) x else x)
            
            monad{ // possible that the player trying to send a message is still in the queue and not in an actual game
                let! arena = Lobby.arenas |> List.tryFind (PersistentHashMap.containsKey' this.ID) |> Option.map PersistentHashMap.toSeq |> Option.map List.ofSeq
                let ids = arena |> List.map fst
                let arena_inps = arena |> List.map snd
                if (List.forall (Option.isSome) arena_inps) then do
                    GD.Print "Sending to all"
                    this.send_to (ids) (ServerState(arena_inps |> List.choose id, DateTime.Now)(*.ToAscii()*))
                    Lobby.arenas <- Lobby.arenas |> List.map (fun x -> if PersistentHashMap.containsKey' this.ID x then x |> PersistentHashMap.map (fun x -> None) else x)
            } |> ignore
        | _ ->
            GD.Print "Other Message"
            ()
    override this.OnClose(e) =
        GD.Print "Someone closed connection"
        for (id, _) in Lobby.arenas |> List.find ( fun x -> PersistentHashMap.containsKey' this.ID x) |> PersistentHashMap.map id do this.Sessions.CloseSession id  
        Lobby.arenas <- Lobby.arenas |> List.filter (fun x -> not (PersistentHashMap.containsKey' this.ID x))
        Lobby.queue <- Lobby.queue |> List.filter (fun x -> not (this.ID=x))
    override this.OnError(e) =
        GD.Print ("Connection error: ", e.Message)
        for (id, _) in Lobby.arenas |> List.find ( fun x -> PersistentHashMap.containsKey' this.ID x) |> PersistentHashMap.map id do this.Sessions.CloseSession id  
        Lobby.arenas <- Lobby.arenas |> List.filter (fun x -> not (PersistentHashMap.containsKey' this.ID x))
        Lobby.queue <- Lobby.queue |> List.filter (fun x -> not (this.ID=x))
    member this.send_to (ids) (msg: Message) =
        let msg = JsonConvert.SerializeObject(msg)
        for id in ids do this.Sessions.SendToAsync(msg, id, null)
    member this.send (msg: Message) =
        this.SendAsync(JsonConvert.SerializeObject(msg), null)
    member this.broadcast (msg: Message) =
        this.Sessions.BroadcastAsync(JsonConvert.SerializeObject(msg), null)
        
and LobbyFs() =
    inherit Node2D()
and ServerFs() =
    inherit Node()
    static member val ws = lazy (
        let ret = new WebSocketServer(8080, true)
        ret.AddWebSocketService<Lobby>("/lobby")
        ret.SslConfiguration.ServerCertificate <- X509Certificate2("/certificate.pfx")
        GD.Print ret.SslConfiguration.ServerCertificate
        ret.Start()
        GD.Print("Server started")
        ret
    )
    
    override this._Ready() =
        ServerFs.ws.Value |> ignore

