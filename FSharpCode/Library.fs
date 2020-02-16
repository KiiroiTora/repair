namespace FSharpCode

open Godot

type ObjectType = LH | RH | RL | LL | H | AXE
module Exts =

    type AudioStreamPlayer with
        member this.Play'() =
            this.PitchScale <- float32 <| GD.RandRange(0.9, 1.1)
            this.Play()

    type Node with
        member this.GetNode'<'a when 'a :> Node>(path: string) =
            lazy (this.GetNode(new NodePath(path)) :?> 'a)

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
    type Godot.Collections.Array with 
        member this.Empty() = this.Count > 0
        member this.PopFront() = 
            let ret = this.Item 0
            this.RemoveAt 0
            ret
        member this.PushFront x = this.Insert(0, x)
[<Struct>]
type OptionalBuilder =
  member __.Bind(opt, binder) =
    match opt with
    | Some value -> binder value
    | None -> None
  member __.Return(value) =
    Some value
  member __.Zero() =
    None
  member __.Combine(a, b) = 
    match a, b with 
    | Some a', Some b' -> Some(a', b')
    | Some a', None -> None
    | None, Some b' -> None
    | None, None -> None
  member __.Delay(f) =
    f()  

type Part = {
    Type: System.Type;
    Format: int option;
    Name: string;
    Part: int16;
    Number: float32;
    FileName: string option
}

type PartResult = {
    ty: System.Type;
    format: string;
    name: string;
    part: int16;
    number: float32;
    fileName: string
}


