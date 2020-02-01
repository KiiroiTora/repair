extends KinematicBody2D

signal pick_up(type)

var velocity : Vector2
var speed : float = 600
onready var rot_speed = rand_range(360*2, 360*5) 
onready var duration : float = 1#rand_range(0.1, 0.5)

var elapsed : float = 0.0
var can_pick_up = false

var type = "AXE"

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

func _on_Area2D_area_entered(area):

	if area.get_parent().has_method("slice") && !can_pick_up:
		area.get_parent().slice()
	pass # Replace with function body.
