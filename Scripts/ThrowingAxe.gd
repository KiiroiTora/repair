extends KinematicBody2D

signal pick_up(type)

var velocity : Vector2
var speed : float = 600
onready var rot_speed = rand_range(360*2, 360*5) 
onready var duration : float = 1#rand_range(0.1, 0.5)
onready var disappearing_sprite = preload("res://Scenes/DisappearingSprite.tscn")

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
		
		
		var s1 = disappearing_sprite.instance()
		s1.texture = $Sprite.texture
		s1.global_position = global_position
		s1.rotation = $Sprite.rotation
		s1.z_index = -1
		
		get_parent().add_child(s1)

func _on_Area2D_area_entered(area):

	if area.get_parent().has_method("slice") && !can_pick_up:
		area.get_parent().slice()
	pass # Replace with function body.
