using Godot;
using System;

public partial class Camera : Camera3D
{
    [Export]
    MeshInstance3D panel=null;

    [Export]
    float MouseSensitivity { get; set; } = 0.8f;
    [Export(PropertyHint.Range, "0,90")]
    float TotalPitch { get; set; } = 85f;
    Vector2 mouseMotion = Vector2.Zero;
	float moveSpeed = 0.1f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{


        Vector3 input = Vector3.Zero;

        if (Input.IsActionPressed("ui_up"))
        {
            input += new Vector3(0, 0, -1);
        }

        if (Input.IsActionPressed("ui_left"))
        {
            input += new Vector3(-1, 0, 0);
        }

        if (Input.IsActionPressed("ui_right"))
        {
            input += new Vector3(1, 0, 0);
        }

        if (Input.IsActionPressed("ui_down"))
        {
            input += new Vector3(0, 0, 1);
        }

        if (Input.IsActionPressed("c_up"))
        {
            input += new Vector3(0, 1, 0);
        }

        if (Input.IsActionPressed("c_down"))
        {
            input += new Vector3(0, -1, 0);
        }

        Vector3 Velocity = input.Normalized() * moveSpeed;
        //Position += Velocity;
        input = Vector3.Zero;
        Translate(Velocity);
		mouseMotion = Vector2.Zero;


        if (panel != null) {
            QuadMesh q = panel.Mesh as QuadMesh;
            if (q != null)
            {
                ShaderMaterial sm = q.Material as ShaderMaterial;
                if (sm != null)
                {
                    GD.Print(Position);
                    sm.SetShaderParameter("CameraPosition", this.Position);
                    sm.SetShaderParameter("CameraRotation", new Vector3(-GlobalRotation.X, GlobalRotation.Y, 0));
                }
            }
        }
    }


    public override void _Input(InputEvent @event)
    {

        if (@event is InputEventMouseButton mouseEvent) {
			switch (mouseEvent.ButtonIndex)
			{
				case MouseButton.Right:
					Input.MouseMode = mouseEvent.Pressed ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
					break;
				case MouseButton.WheelUp:
					moveSpeed += 0.2f;
					break;
				case MouseButton.WheelDown: 
					moveSpeed -= 0.2f;
					break;
				default:
					break;
			}
		}

        if (@event is InputEventMouseMotion)
        {
            if (Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                Vector2 motion = ((InputEventMouseMotion)@event).Relative *= MouseSensitivity;
                float yaw = motion.X;
                float pitch = motion.Y;

                pitch = Mathf.Clamp(pitch, -90 - TotalPitch, 90 - TotalPitch);
                if (Rotation.X > Mathf.Pi / 2f && pitch < 0f) pitch = 0f;
                if (Rotation.X < -Mathf.Pi / 2f && pitch > 0f) pitch = 0f;
                RotateY(Mathf.DegToRad(-yaw));
                RotateObjectLocal(new Vector3(1, 0, 0), Mathf.DegToRad(-pitch));
                GD.Print($"rot {Mathf.RadToDeg(Rotation.X)} rot rad {Rotation.X}  pitch {pitch}");
            }
        }
    }
}
