extends KinematicBody2D
class_name ThrowingAxe
signal pick_up(type)

export var flying_curve: Curve

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
		var coll = move_and_collide(velocity * speed *delta * flying_curve.interpolate(elapsed/duration))
		if coll:
			velocity = velocity.bounce(coll.normal)
			$impact_sound.pitch_scale = rand_range(0.9, 1.1)
			$impact_sound.play()
		$Sprite.rotate(deg2rad(rot_speed) * delta * flying_curve.interpolate(elapsed/duration))
		
		
		var s1 = disappearing_sprite.instance()
		s1.texture = $Sprite.texture
		s1.global_position = global_position
		s1.rotation = $Sprite.rotation
		s1.z_index = -1
		
		get_parent().add_child(s1)
	if can_pick_up:
		$Area2D.set_deferred("monitorable", true)
func _on_Area2D_area_entered(area):

	if area.get_parent().has_method("slice") && !can_pick_up:
		area.get_parent().slice()
	pass # Replace with function body.
