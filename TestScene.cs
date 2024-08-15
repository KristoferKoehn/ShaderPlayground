using Godot;
using System;

public partial class TestScene : Node2D
{


    [Export(PropertyHint.Range, "0,90")]
    public float TotalPitch { get; set; } = 85f;
    public Vector2 mouseMotion = Vector2.Zero;
    public float moveSpeed = 40.0f;


    [Export] float _NearPlane = 0.1f;
    [Export] float _FarPlane = 8;
    [Export] float FOV = 1;
    [Export] float AspectRatio = 1;


    [Export] public float MouseSensitivity { get; set; } = 0.2f;
    [Export] Polygon2D ColorRect { get; set; }
    [Export] Sprite2D visuals { get; set; }
    [Export] Sprite2D Copyvisuals { get; set; }
    [Export] Node3D FakeNode { get; set; }

    Projection ViewMatrix = new Projection();
    Projection InvViewMatrix = new Projection();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

        if (visuals != null)
        {
            ShaderMaterial sm = visuals.Material as ShaderMaterial;
            if (sm != null)
            {

                float theta_x = -FakeNode.GlobalRotation.X;
                float theta_y = -FakeNode.GlobalRotation.Y;
                float theta_z = -FakeNode.GlobalRotation.Z;


                Projection XRotationMatrix = new Projection();
                XRotationMatrix.X = new Vector4(1, 0, 0, 0);
                XRotationMatrix.Y = new Vector4(0, Mathf.Cos(theta_x), Mathf.Sin(theta_x), 0);
                XRotationMatrix.Z = new Vector4(0, -Mathf.Sin(theta_x), Mathf.Cos(theta_x), 0);
                XRotationMatrix.W = new Vector4(0, 0, 0, 1);

                Projection YRotationMatrix = new Projection();
                YRotationMatrix.X = new Vector4(Mathf.Cos(theta_y), 0, -Mathf.Sin(theta_y), 0);
                YRotationMatrix.Y = new Vector4(0, 1, 0, 0);
                YRotationMatrix.Z = new Vector4(Mathf.Sin(theta_y), 0, Mathf.Cos(theta_y), 0);
                YRotationMatrix.W = new Vector4(0, 0, 0, 1);

                Projection ZRotationMatrix = new Projection();
                ZRotationMatrix.X = new Vector4(Mathf.Cos(theta_z), -Mathf.Sin(theta_z), 0, 0);
                ZRotationMatrix.Y = new Vector4(Mathf.Sin(theta_z), Mathf.Cos(theta_z), 0, 0);
                ZRotationMatrix.Z = new Vector4(0, 0, 1, 0);
                ZRotationMatrix.W = new Vector4(0, 0, 0, 1);

                Projection RotationMatrix = ZRotationMatrix * XRotationMatrix * YRotationMatrix;


                Projection TranslationMatrix = new Projection();
                TranslationMatrix.X = new Vector4(1, 0, 0, 0);
                TranslationMatrix.Y = new Vector4(0, 1, 0, 0);
                TranslationMatrix.Z = new Vector4(0, 0, 1, 0);
                TranslationMatrix.W = new Vector4(-FakeNode.GlobalPosition.X, -FakeNode.GlobalPosition.Y, -FakeNode.GlobalPosition.Z, 1);


                ViewMatrix = RotationMatrix * TranslationMatrix;
                InvViewMatrix = (RotationMatrix * TranslationMatrix).Inverse();

                //sm.SetShaderParameter("CameraPosition", Position);
                sm.SetShaderParameter("CameraRotation", new Vector3(FakeNode.GlobalRotation.X, FakeNode.GlobalRotation.Y, 0));
                sm.SetShaderParameter("ViewMatrix", ViewMatrix);
                sm.SetShaderParameter("InvViewMatrix", InvViewMatrix);
                sm.SetShaderParameter("NearPlane", _NearPlane);
                sm.SetShaderParameter("FarPlane", _FarPlane);
                sm.SetShaderParameter("FOV", FOV);
                sm.SetShaderParameter("AspectRatio", AspectRatio);

            }
        }

        if (!Engine.IsEditorHint())
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

            Vector3 Velocity = input.Normalized() * moveSpeed * (float)delta;
            //Position += Velocity;
            input = Vector3.Zero;
            FakeNode.Translate(Velocity);

        }
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
                    moveSpeed += 2f;
                    break;
                case MouseButton.WheelDown:
                    moveSpeed -= 2f;
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
                if (FakeNode.Rotation.X > Mathf.Pi / 2f && pitch < 0f) pitch = 0f;
                if (FakeNode.Rotation.X < -Mathf.Pi / 2f && pitch > 0f) pitch = 0f;
                FakeNode.RotateY(Mathf.DegToRad(-yaw));
                FakeNode.RotateObjectLocal(new Vector3(1, 0, 0), Mathf.DegToRad(-pitch));
            }
        }
    }
}
