using Godot;
using System;

[Tool]
public partial class DepthBurn : Node3D
{
	[Export] Vector4 Plane {  get; set; } = Vector4.Zero;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		ShaderMaterial sm = ResourceLoader.Load<ShaderMaterial>("res://Materials/DepthBurn.tres");
		Vector3 normal = new Vector3(Plane.X, Plane.Y, Plane.Z);
		sm.SetShaderParameter("PlaneNormal", normal);
        sm.SetShaderParameter("PlaneOffset", Plane.W);
    }
}
