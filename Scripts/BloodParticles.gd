extends Node2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

var follow
# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):#	pass
	if follow:
		var wr = weakref(follow)
		if (wr.get_ref()):
			global_position = follow.global_position
			global_rotation = follow.get_node("Sprite").global_rotation
		else:
			$BloodParticles.emitting = false
	pass


func _on_Timer_timeout():
	queue_free()
	pass # Replace with function body.


