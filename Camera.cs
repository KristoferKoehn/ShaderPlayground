using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class Camera : Camera3D
{
    [Export]
    MeshInstance3D panel=null;

    [Export]
    MeshInstance3D NearPlane = null;

    [Export]
    MeshInstance3D FarPlane = null;

    [Export]
    float MouseSensitivity { get; set; } = 0.2f;

    [Export] MeshInstance3D Ball2;
    [Export] MeshInstance3D RayMarker1;
    [Export] MeshInstance3D RayMarker2;
    [Export] MeshInstance3D RayMarker3;
    [Export] MeshInstance3D RayMarker4;


    [Export] MeshInstance3D RayOrigin1;
    [Export] MeshInstance3D RayOrigin2;
    [Export] MeshInstance3D RayOrigin3;
    [Export] MeshInstance3D RayOrigin4;


    [Export] MeshInstance3D LocalToWorld;


    [Export(PropertyHint.Range, "0,90")]
    float TotalPitch { get; set; } = 85f;
    Vector2 mouseMotion = Vector2.Zero;
	float moveSpeed = 40.0f;


    [Export] float _NearPlane = 0.1f;
    [Export] float _FarPlane = 8;
    [Export] float FOV = 1;
    [Export] float AspectRatio = 1;

    Vector3 TestPosition = Vector3.Zero;
    Projection ViewMatrix = new Projection();
    Projection InvViewMatrix = new Projection();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{


        if (panel != null) {
            QuadMesh q = panel.Mesh as QuadMesh;
            if (q != null)
            {
                ShaderMaterial sm = q.Material as ShaderMaterial;
                if (sm != null)
                {

                    float theta_x = -GlobalRotation.X;
                    float theta_y = -GlobalRotation.Y;
                    float theta_z = -GlobalRotation.Z;


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
                    TranslationMatrix.W = new Vector4(-GlobalPosition.X, -GlobalPosition.Y, -GlobalPosition.Z, 1);


                    ViewMatrix = RotationMatrix * TranslationMatrix;
                    InvViewMatrix = (RotationMatrix * TranslationMatrix).Inverse();

                    //sm.SetShaderParameter("CameraPosition", Position);
                    sm.SetShaderParameter("CameraRotation", new Vector3(GlobalRotation.X, GlobalRotation.Y, 0));
                    sm.SetShaderParameter("ViewMatrix", ViewMatrix);
                    sm.SetShaderParameter("InvViewMatrix", InvViewMatrix);
                    sm.SetShaderParameter("NearPlane", _NearPlane);
                    sm.SetShaderParameter("FarPlane", _FarPlane);
                    sm.SetShaderParameter("FOV", FOV);
                    sm.SetShaderParameter("AspectRatio", AspectRatio);
                    
                    Array<Vector3> points = (Array<Vector3>)sm.GetShaderParameter("points");
                    points[1] = Ball2.GlobalPosition;
                    sm.SetShaderParameter("points", points);

                    Array<float> radii = (Array<float>)sm.GetShaderParameter("radii");
                    radii[1] = ((SphereMesh)Ball2.Mesh).Radius;
                    sm.SetShaderParameter("radii", radii);

                    Vector4 passnear = (RotationMatrix * TranslationMatrix).Inverse() * new Vector4(0, 0, -_NearPlane, 1);
                    Vector4 passfar = (RotationMatrix * TranslationMatrix).Inverse() * new Vector4(0, 0, -_FarPlane, 1);

                    Vector3 nearPlane = new Vector3(passnear.X, passnear.Y, passnear.Z);
                    Vector3 farPlane = new Vector3(passfar.X, passfar.Y, passfar.Z);

                    NearPlane.GlobalPosition = nearPlane;
                    FarPlane.GlobalPosition = farPlane;

                    float x_range = (2.0f * 1.0f - 1.0f) - (2.0f * 0 - 1.0f);
                    float y_range = (1.0f - 2.0f * 1.0f) - (1.0f - 2.0f * 0);
                    float x = x_range * Mathf.Tan(FOV / 2.0f) * AspectRatio;
                    float y = -y_range * Mathf.Tan(FOV / 2.0f);

                    //vec3(x_ndc * tan(FOV / 2.0) * AspectRatio, -y_ndc * tan(FOV / 2.0), -FarPlane);
                    //float x_ndc = 2.0 * uv.x - 1.0;
                    //float y_ndc = 1.0 - 2.0 * uv.y;
                    Vector3 coord = new();
                    Vector4 ray = new();
                    coord = GetFarPlane(new Vector2(0, 0));
                    ray = InvViewMatrix * new Vector4(coord.X, coord.Y, coord.Z, 1.0f);
                    RayMarker1.GlobalPosition = new Vector3(ray.X, ray.Y, ray.Z);

                    coord = GetFarPlane(new Vector2(1.0f, 0));
                    ray = InvViewMatrix * new Vector4(coord.X, coord.Y, coord.Z, 1.0f);

                    RayMarker2.GlobalPosition = new Vector3(ray.X, ray.Y, ray.Z);

                    coord = GetFarPlane(new Vector2(0.0f, 1.0f));
                    ray = InvViewMatrix * new Vector4(coord.X, coord.Y, coord.Z, 1.0f);

                    RayMarker3.GlobalPosition = new Vector3(ray.X, ray.Y, ray.Z);

                    coord = GetFarPlane(new Vector2(1.0f, 1.0f));
                    ray = InvViewMatrix * new Vector4(coord.X, coord.Y, coord.Z, 1.0f);
                    RayMarker4.GlobalPosition = new Vector3(ray.X, ray.Y, ray.Z);


                    ray = GetNearPlane(new Vector2(0.0f, 0.0f));
                    RayOrigin1.GlobalPosition = InvViewMatrix * new Vector3(ray.X, ray.Y, ray.Z);

                    ray = GetNearPlane(new Vector2(1.0f, 0.0f));
                    RayOrigin2.GlobalPosition = InvViewMatrix * new Vector3(ray.X, ray.Y, ray.Z);

                    ray = GetNearPlane(new Vector2(0.0f, 1.0f));
                    RayOrigin3.GlobalPosition = InvViewMatrix * new Vector3(ray.X, ray.Y, ray.Z);

                    ray = GetNearPlane(new Vector2(1.0f, 1.0f));
                    RayOrigin4.GlobalPosition = InvViewMatrix * new Vector3(ray.X, ray.Y, ray.Z);

                    ((QuadMesh)NearPlane.Mesh).Size = new Vector2(x/ Mathf.Tan(FOV / 2.0f), y/ Mathf.Tan(FOV / 2.0f));

                    ((QuadMesh)FarPlane.Mesh).Size = new Vector2(x,y);

                    

                }
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
            Translate(Velocity);

        }

    }

    Vector3 GetFarPlane(Vector2 uv)
    {
        float x_ndc = 2.0f * uv.X - 1.0f;
        float y_ndc = 1.0f - 2.0f * uv.Y;

        Vector3 farPlane = new Vector3(x_ndc * Mathf.Tan(FOV / 2.0f) * AspectRatio, y_ndc * Mathf.Tan(FOV / 2.0f), -_FarPlane);
        return farPlane;
    }

    Vector4 GetNearPlane(Vector2 uv)
    {

        float x_ndc = 2.0f * uv.X - 1.0f;
        float y_ndc = 1.0f - 2.0f * uv.Y;

        Vector4 near = new Vector4(x_ndc * AspectRatio, y_ndc, -_NearPlane, 1.0f);
        return near;
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
                if (Rotation.X > Mathf.Pi / 2f && pitch < 0f) pitch = 0f;
                if (Rotation.X < -Mathf.Pi / 2f && pitch > 0f) pitch = 0f;
                RotateY(Mathf.DegToRad(-yaw));
                RotateObjectLocal(new Vector3(1, 0, 0), Mathf.DegToRad(-pitch));
            }
        }
    }
}
