extends KinematicBody2D

export var type = "LH"
export var flying_curve: Curve

onready var rot_speed : float = rand_range(360*4, 360*8) 
onready var duration : float = rand_range(0.5, 1.5)

signal pick_up(type)

var velocity : Vector2
var elapsed : float = 0.0
var can_pick_up = true

var particles : Particles2D

func _ready():
	#$Sprite/CPUParticles2D.emitting = true
	velocity = Vector2(rand_range(-1, 1), rand_range(-1, 1)).normalized() * rand_range(600, 900)

func _process(delta):	
	elapsed += delta
	if particles != null:
		print("Know blood")
		particles.global_position = $Sprite/BloodParticles.global_position
		particles.global_rotation = $Sprite/BloodParticles.global_rotation
	if elapsed >= duration:
		velocity = Vector2.ZERO

	else:
		var coll = move_and_collide(velocity*delta*flying_curve.interpolate(elapsed/duration))
		if !coll:
			$Sprite.rotate(deg2rad(rot_speed) * delta * flying_curve.interpolate(elapsed/duration))
	$Area2D.set_deferred("monitorable", velocity == Vector2.ZERO)
	
func pick_up():
	#if particles:
		#particles.emitting = false
	
	#get_parent().get_parent().add_child(particles)
	#particles.z_index = 5
	pass
