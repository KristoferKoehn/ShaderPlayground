@tool
extends Node3D


@export var plane_normal = Vector3(1,0,0)
# Called when the node enters the scene tree for the first time.
func _ready():
	pass

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	var children : Array[Node] = get_children()
	for c in children:
		
		if (c.material_override == null):
			var sh = ShaderMaterial.new()
			sh.shader = load("res://Shaders/DepthBurn.gdshader")
			c.material_override = sh
		c.material_override.set_shader_parameter("plane_normal",plane_normal)
