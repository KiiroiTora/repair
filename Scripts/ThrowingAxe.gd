extends KinematicBody2D

signal pick_up(type)

var velocity : Vector2
var speed : float = 300
onready var rot_speed = rand_range(360*4, 360*8) 
onready var duration : float = rand_range(0.1, 0.5)

var elapsed : float = 0.0
var can_pick_up = false

func _process(delta):	
	elapsed += delta
	if elapsed >= duration:
		velocity = Vector2.ZERO
		can_pick_up = true
	else:
		var coll = move_and_collide(velocity * speed *delta)
		if coll:
			velocity = velocity.bounce(coll.normal)
		$Sprite.rotate(deg2rad(rot_speed) * delta)

func _on_Area2D_body_entered(body):
	if can_pick_up:
		emit_signal("pick_up", "AXE")
	else:
		if body.has_method("slice"):
			body.slice()
