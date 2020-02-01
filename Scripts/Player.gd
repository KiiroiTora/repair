extends KinematicBody2D

export var pid = "1"

onready var anim := $body/anim_body
onready var body := $body
onready var head := $body/head
onready var l_hand := $body/l_hand
onready var l_axe := $body/l_hand/axe 
onready var r_hand := $body/r_hand
onready var r_axe := $body/r_hand/axe
onready var l_leg := $body/l_leg
onready var r_leg := $body/r_leg

export var flying_object_scene = preload("res://Scenes/FlyingObject.tscn")
export var throwing_axe_scene = preload("res://Scenes/ThrowingAxe.tscn")

onready var body_parts := {
	"H" : head,
	"LH" : l_hand,
	"RH" : r_hand,
	"LL" : l_leg,
	"RL" : r_leg,
}

var limbs := ["LH", "LL", "RL", "RH", "H"]

var speed := 300.0
var velocity := Vector2.ZERO

func _ready():
	randomize()
	
	pass # Replace with function body.

func _process(delta):
	
	velocity = Vector2(Input.get_action_strength("right" + pid) - Input.get_action_strength("left" + pid), Input.get_action_strength("down" + pid) - Input.get_action_strength("up" + pid)).normalized() * speed
	velocity = move_and_slide(velocity)
	
	body.scale.x = abs(body.scale.x) if velocity.x > 0 else (- abs(body.scale.x) if velocity.x < 0 else body.scale.x)
	
	anim.play(("idle" if velocity.length() == 0 else "walk") if !Input.is_action_pressed("throw" + pid) else "swing")
	
	for b_part in body_parts.keys():
		body_parts[b_part].visible = limbs.has(b_part)
		
		
func slice():
	var removed = limbs.pop_front()
	var fl_obj = flying_object_scene.instance()
	fl_obj.get_node("Sprite").texture = body_parts[removed].texture
	get_parent().add_child(fl_obj)

func throw():
	pass
