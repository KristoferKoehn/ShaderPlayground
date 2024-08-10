using Godot;
using System;

[Tool]
public partial class Main : Node3D
{
	[Export] Node3D CarouselGimbal { get; set; }
	[Export] bool RotateLeft { get; set; }
	[Export] bool RotateRight { get; set; }

	[Export] MeshInstance3D Teleport1 { get; set; }
    [Export] MeshInstance3D Teleport2 { get; set; }
	[Export] MeshInstance3D TeleportSubject1 { get; set; }
	[Export] MeshInstance3D TeleportSubject2 { get; set; }


    float RotationState = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Teleport1.Position = new Vector3(-1.5f, 1, -0.5f);
        Teleport2.Position = new Vector3( 1.5f, 1, -0.5f);
		
        if (!Engine.IsEditorHint())
        {
            TweenDown(Teleport1);
            TweenDown(Teleport2);
        }        
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (RotateLeft)
		{
			RotationState += Mathf.Pi / 9.0f;

            RotateLeft = false;
			//do the thing
            Quaternion q = new Quaternion(Vector3.Up, RotationState);
            Tween tween = GetTree().CreateTween();
            tween.TweenProperty(CarouselGimbal, "quaternion", q * Quaternion, 0.3f).SetTrans(Tween.TransitionType.Back);
        }

		if (RotateRight)
		{
            RotationState -= Mathf.Pi / 9.0f;
            RotateRight = false;
            //do the other thing

            Quaternion q = new Quaternion(Vector3.Up, RotationState);
            Tween tween = GetTree().CreateTween();
            tween.TweenProperty(CarouselGimbal, "quaternion", q * Quaternion, 0.3f).SetTrans(Tween.TransitionType.Back);
        }

		ShaderMaterial SM1 = (ShaderMaterial)TeleportSubject1.GetSurfaceOverrideMaterial(0);
		SM1.SetShaderParameter("PlaneOffset", -Teleport1.Position.Y);

        ShaderMaterial SM2 = (ShaderMaterial)TeleportSubject2.GetSurfaceOverrideMaterial(0);
        SM2.SetShaderParameter("PlaneOffset", Teleport2.Position.Y);

    }

    public void RotateCounterClockwise()
    {

    }


	public void TweenDown(Node3D node)
	{
		Tween t = GetTree().CreateTween();
		t.TweenProperty(node, "position", new Vector3(node.Position.X, -1, node.Position.Z), 1.4);
		t.Finished += () =>
		{
            GetTree().CreateTimer(1.0).Timeout += () => TweenUp(node);
        };
	}

    public void TweenUp(Node3D node)
    {
        Tween t = GetTree().CreateTween();
        t.TweenProperty(node, "position", new Vector3(node.Position.X, 1, node.Position.Z), 1.4);
        t.Finished += () =>
        {
			GetTree().CreateTimer(1.0).Timeout += () => TweenDown(node);
        };
    }

}