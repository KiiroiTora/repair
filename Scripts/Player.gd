extends KinematicBody2D

export var pid = "1"


export var throw_strength : Curve
export var max_throw_duration := 2

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

var throw_time := 0.0

var has_axe_l = true
var has_axe_r = true

func _ready():
	body.texture = load("res://Images/V.2/Player "+pid+"/Torso.png")
	head.texture = load("res://Images/V.2/Player "+pid+"/Head.png")
	l_leg.texture = load("res://Images/V.2/Player "+pid+"/L_Leg.png")
	r_leg.texture = load("res://Images/V.2/Player "+pid+"/R_Leg.png")
	randomize()

func _process(delta):
	
	if Input.is_action_just_released("throw" + pid):
		throw()
	
	velocity = controller_dir()
	velocity = move_and_slide(velocity * speed) if !(Input.is_action_pressed("throw" + pid) and (has_axe_l or has_axe_r)) else Vector2.ZERO
	
	throw_time = clamp(throw_time + delta, 0, max_throw_duration)/max_throw_duration if Input.is_action_pressed("throw" + pid) and (has_axe_l or has_axe_r) else 0
	
	body.scale.x = abs(body.scale.x) if velocity.x > 0 else (- abs(body.scale.x) if velocity.x < 0 else body.scale.x)
	
	anim.play(("idle" if velocity.length() == 0 else "walk") if !(Input.is_action_pressed("throw" + pid) and (has_axe_l or has_axe_r)) else "swing"+("l" if has_axe_l else "r"))
	
	if Input.is_action_pressed("throw" + pid):
		if has_axe_l:
			l_hand.look_at(controller_dir() + l_hand.global_position)
		elif has_axe_r:
			r_hand.look_at(controller_dir() + r_hand.global_position)
		
	
	for b_part in body_parts.keys():
		body_parts[b_part].visible = limbs.has(b_part)
		
	$body/l_hand/axe.visible = has_axe_l
	$body/r_hand/axe.visible = has_axe_r
		
func slice():
	var removed = limbs.pop_front()
	var fl_obj = flying_object_scene.instance()
	
	has_axe_l = false if removed == "LH" else has_axe_l
	has_axe_r = false if removed == "RH" else has_axe_r
	
	fl_obj.type = removed
	fl_obj.global_position = global_position
	fl_obj.get_node("Sprite").texture = body_parts[removed].texture
	get_parent().add_child(fl_obj)

func throw():
	print("throwing")
	var axe = throwing_axe_scene.instance()
	if has_axe_l:
		has_axe_l = false
	elif has_axe_r:
		has_axe_r = false
	$Position2D.look_at($Position2D.global_position + controller_dir())
	axe.global_position = $Position2D/Position2D.global_position
	axe.velocity = ($Position2D/Position2D.global_position - $Position2D.global_position).normalized()
	axe.speed = throw_strength.interpolate(throw_time) * 10000 + 400
	get_parent().add_child(axe)
	pass

func controller_dir():
	return Vector2(Input.get_action_strength("right" + pid) - Input.get_action_strength("left" + pid), Input.get_action_strength("down" + pid) - Input.get_action_strength("up" + pid)).normalized()


func _on_area_pickup_area_entered(area):
	var obj = area.get_parent()
	if "type" in obj && "can_pick_up" in obj && obj.can_pick_up:
		if obj.type != "AXE" && !limbs.has(obj.type):
			limbs.push_front(obj.type)
			obj.queue_free()
		elif obj.type == "AXE":
			if limbs.has("LH") and limbs.has("RH"):
				if !has_axe_r and !has_axe_l:
					has_axe_l = true
					obj.queue_free()
				elif has_axe_l and !has_axe_r:
					has_axe_r = true
					obj.queue_free()
				elif !has_axe_l and has_axe_r:
					has_axe_l = true
					obj.queue_free()
			elif !limbs.has("LH"):
				if limbs.has("RH") and !has_axe_r:
					has_axe_r = true
					obj.queue_free()
			elif !limbs.has("RH"):
				if limbs.has("LH") and !has_axe_l:
					has_axe_l = true
					obj.queue_free()
			
					
					
				
		
	pass # Replace with function body.
