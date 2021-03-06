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
export var winscreen = preload("res://Scenes/Winner.tscn")
export var particles_tscn = preload("res://Scenes/BloodParticles.tscn")
onready var body_parts := {
	"H" : head,
	"LH" : l_hand,
	"RH" : r_hand,
	"LL" : l_leg,
	"RL" : r_leg,
}

var limbs := ["LH", "LL", "RL", "RH", "H"]

var speed := 350.0 
var velocity := Vector2.ZERO

var throw_time := 0.0

var has_axe_l = true
var has_axe_r = true

func _ready():
#	body.texture = load("res://Images/V.2/Player "+pid+"/Torso.png")
#	head.texture = load("res://Images/V.2/Player "+pid+"/Head.png")
#	l_leg.texture = load("res://Images/V.2/Player "+pid+"/L_Leg.png")
#	r_leg.texture = load("res://Images/V.2/Player "+pid+"/R_Leg.png")
	
	body.texture = load("res://Images/Character V.3/P"+pid+"/Torso.png")
	head.texture = load("res://Images/Character V.3/P"+pid+"/Head.png")
	l_leg.texture = load("res://Images/Character V.3/P"+pid+"/L_Leg.png")
	r_leg.texture = load("res://Images/Character V.3/P"+pid+"/R_Leg.png")
	randomize()

func _process(delta):
	
	if Input.is_action_just_pressed("restart"):
		get_tree().reload_current_scene()
	
	for b_part in body_parts.keys():
		body_parts[b_part].visible = limbs.has(b_part)
	if limbs.empty():
		return
	if Input.is_action_just_released("throw" + pid):
		throw()
	
	velocity = controller_dir()
	velocity = move_and_slide(velocity * (2.5 if Input.is_action_pressed("run" + pid) else 1) * (speed if limbs.has("LL") and limbs.has("RL") else (speed/2 if limbs.has("LL") or limbs.has("RL") else speed/4)) ) if !(Input.is_action_pressed("throw" + pid) and (has_axe_l or has_axe_r)) else Vector2.ZERO
	
	$body/anim_body.playback_speed = 4 if Input.is_action_pressed("run" + pid) else 2
	
	throw_time = clamp(throw_time + delta, 0, max_throw_duration) if Input.is_action_pressed("throw" + pid) and (has_axe_l or has_axe_r) else 0

	$body/l_hand/axe.animation = "default" if !(Input.is_action_pressed("throw" + pid) and has_axe_l) else "charge"
	$body/l_hand/axe.frame = 0 if !(Input.is_action_pressed("throw" + pid) and has_axe_l) else int((throw_time/max_throw_duration) * 8)
	$body/l_hand/axe.scale = Vector2.ONE if !(Input.is_action_pressed("throw" + pid) and has_axe_l) else Vector2.ONE * ((throw_time/max_throw_duration)/5) + Vector2.ONE 

	$body/r_hand/axe.animation = "default" if !(Input.is_action_pressed("throw" + pid) and has_axe_r and !has_axe_l) else "charge"
	$body/r_hand/axe.frame = 0 if !(Input.is_action_pressed("throw" + pid) and has_axe_r and !has_axe_l) else int((throw_time/max_throw_duration) * 8)
	$body/r_hand/axe.scale = Vector2.ONE if !(Input.is_action_pressed("throw" + pid) and has_axe_r and !has_axe_l) else Vector2.ONE * ((throw_time/max_throw_duration)/5) + Vector2.ONE 

	

	
	
	
	$particles_throw.emitting = throw_time > 0.25
	$particles_throw.speed_scale = 1 + throw_strength.interpolate(throw_time/max_throw_duration) * 8
	$particles_throw.color = Color.yellow if throw_time >= max_throw_duration*0.95 else Color.green
	$shadow.position.y = 137.669 if limbs.has("LL") or limbs.has("RL") else 60.0
	body.scale.x = abs(body.scale.x) if controller_dir().x > 0 else (- abs(body.scale.x) if controller_dir().x < 0 else body.scale.x)
	
	anim.play(("idle" if velocity.length() == 0 else "walk") if !(Input.is_action_pressed("throw" + pid) and (has_axe_l or has_axe_r)) else "swing"+("l" if has_axe_l else "r"))
	
	if Input.is_action_pressed("throw" + pid):
		if has_axe_l:
			l_hand.look_at(controller_dir().rotated(deg2rad(-150)) + l_hand.global_position)
		elif has_axe_r:
			r_hand.look_at(controller_dir().rotated(deg2rad(-150)) + r_hand.global_position)
		
	
		
	$body/l_hand/axe.visible = has_axe_l
	$body/r_hand/axe.visible = has_axe_r
		
func slice():
	if limbs.empty():
		return
	
	var removed = limbs.pop_front()
	var fl_obj = flying_object_scene.instance()
	
	$audio_slice.pitch_scale = rand_range(0.9, 1.1)
	$audio_slice.play()
	
	
	if (removed == "LH" and has_axe_l) or (removed == "RH" and has_axe_r):
		var axe = flying_object_scene.instance()
		axe.type = "AXE"
		axe.global_position = global_position
		axe.get_node("Sprite").texture = load("res://Images/Weapon Charge/Axe.png")
		get_parent().call_deferred("add_child", axe)
	
	has_axe_l = false if removed == "LH" else has_axe_l
	has_axe_r = false if removed == "RH" else has_axe_r
	
	fl_obj.type = removed
	fl_obj.global_position = global_position

	fl_obj.get_node("Sprite").texture = body_parts[removed].texture
	#var parts = particles_tscn.instance()
	#get_parent().add_child(parts)
	#parts.follow = fl_obj
	get_parent().call_deferred("add_child", fl_obj)
	#fl_obj.set_deferred("particles", parts)
	#fl_obj.get_node("Area2D").set_deferred("monitoring", true)
	#fl_obj.get_node("Sprite/CPUParticles2D").emitting = true
	fl_obj.get_node("Sprite/BloodParticles/BloodParticles").emitting = true
	if limbs.empty():
		die()

func die():
	print("Player " + str(1 if pid == "2" else 2))
	
	
	
	$timer_win.start()
	
func throw():

	if has_axe_l:
		has_axe_l = false
	elif has_axe_r:
		has_axe_r = false
	else:
		return
	$audio_throw.pitch_scale = rand_range(0.9, 1.1)
	$audio_throw.play()
	var axe = throwing_axe_scene.instance()
	$Position2D.look_at(($Position2D.global_position + controller_dir()))
	axe.global_position = $Position2D/Position2D.global_position
	axe.velocity = ($Position2D/Position2D.global_position - $Position2D.global_position).normalized()
	axe.speed = throw_strength.interpolate(throw_time/max_throw_duration) * 2700 + 550 
	axe.scale = Vector2.ONE + Vector2.ONE * (throw_time/max_throw_duration)/5
	axe.get_node("Sprite").texture = load("res://Images/Weapon Charge/Axe.png")
	get_parent().add_child(axe)
	pass

func controller_dir():
	return Vector2(Input.get_action_strength("right" + pid) - Input.get_action_strength("left" + pid), Input.get_action_strength("down" + pid) - Input.get_action_strength("up" + pid)).normalized()


func _on_area_pickup_area_entered(area):
	
	var obj = area.get_parent()
	if "type" in obj && "can_pick_up" in obj && obj.can_pick_up:
		
		if obj.type == "LH" or obj.type == "RH":
			obj.type = "LH" if !limbs.has("LH") else ("RH" if !limbs.has("RH") else obj.type)
		
		if obj.type == "LL" or obj.type == "RL":
			obj.type = "LL" if !limbs.has("LL") else ("RL" if !limbs.has("RL") else obj.type)
		
		if obj.type != "AXE" && !limbs.has(obj.type):
			limbs.push_front(obj.type)
			obj.pick_up()
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


func _on_timer_win_timeout():
	var wscr = winscreen.instance()
	wscr.get_node("CanvasLayer/" + ("1" if pid=="2" else "2")).visible = true
	get_parent().add_child(wscr)
	get_parent().remove_child(self)
	call_deferred("free")

	
	
