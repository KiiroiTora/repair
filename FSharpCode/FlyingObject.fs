module FSharpCode.FlyingObject

open Godot

type FlyingObjectFs() =
    inherit KinematicBody2D()
    member val obj_type = ObjectType.H