extends Node2D


export  var level_scn : PackedScene = preload("res://Scenes/Arena1.tscn")

onready var axe_cursor:= $AxeCursor
onready var anim := $AnimationPlayer

const start_x := 300.809
const start_y := 387.901
const quit_x := 300.809
const quit_y := 593.583



# Called when the node enters the scene tree for the first time.
func _ready():
	
	axe_cursor.position.x = start_x
	axe_cursor.position.y = start_y


func _process(delta):
	
	if get_global_mouse_position().y < (start_y + quit_y)/2:
		axe_cursor.position.y = start_y
		
	if get_global_mouse_position().y >= (start_y + quit_y)/2:

		axe_cursor.position.y = quit_y
		
	if (Input.is_action_just_released("throw1") or Input.is_action_just_released("throw2")) and int(axe_cursor.position.y) == int(start_y):
		anim.play("start")
	elif Input.is_action_just_released("throw1") or Input.is_action_just_released("throw2"):
		anim.play("quit")


func start():
	var wscr = level_scn.instance()
	get_parent().add_child(wscr)
	get_parent().remove_child(self)
	call_deferred("free")

func quit():
	get_tree().quit()
