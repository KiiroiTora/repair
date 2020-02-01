extends KinematicBody2D

export var type = "LH"

onready var rot_speed : float = rand_range(360*4, 360*8) 
onready var duration : float = rand_range(0.1, 0.5)

signal pick_up(type)

var velocity : Vector2
var elapsed : float = 0.0
var can_pick_up = false

func _ready():
	velocity = Vector2(rand_range(-1, 1), rand_range(-1, 1)).normalized() * rand_range(600, 900)

func _process(delta):	
	elapsed += delta
	if elapsed >= duration:
		velocity = Vector2.ZERO
		can_pick_up = true
	else:
		move_and_collide(velocity*delta)
		$Sprite.rotate(deg2rad(rot_speed) * delta)

func _on_Area2D_body_entered(body):
	if can_pick_up:
		emit_signal("pick_up", type)
