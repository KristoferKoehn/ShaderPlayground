using Godot;
using System;

public partial class MouseInputs : Camera3D
{
	[Export]
	Camera Camera { get; set; }


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void _Input(InputEvent @event)
    {

        if (@event is InputEventMouseButton mouseEvent)
        {
            switch (mouseEvent.ButtonIndex)
            {
                case MouseButton.Right:
                    Input.MouseMode = mouseEvent.Pressed ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
                    break;
                case MouseButton.WheelUp:
                    Camera.moveSpeed += 2f;
                    break;
                case MouseButton.WheelDown:
                    Camera.moveSpeed -= 2f;
                    break;
                default:
                    break;
            }
        }

        if (@event is InputEventMouseMotion)
        {
            if (Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                Vector2 motion = ((InputEventMouseMotion)@event).Relative *= Camera.MouseSensitivity;
                float yaw = motion.X;
                float pitch = motion.Y;

                pitch = Mathf.Clamp(pitch, -90 - Camera.TotalPitch, 90 - Camera.TotalPitch);
                if (Camera.Rotation.X > Mathf.Pi / 2f && pitch < 0f) pitch = 0f;
                if (Camera.Rotation.X < -Mathf.Pi / 2f && pitch > 0f) pitch = 0f;
                Camera.RotateY(Mathf.DegToRad(-yaw));
                Camera.RotateObjectLocal(new Vector3(1, 0, 0), Mathf.DegToRad(-pitch));
            }
        }
    }
}
