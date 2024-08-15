using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class Camera : Camera3D
{
    [Export]
    MeshInstance3D panel=null;

    [Export]
    MeshInstance3D NearPlane = null;

    [Export]
    MeshInstance3D FarPlane = null;

    [Export]
    public float MouseSensitivity { get; set; } = 0.2f;

    [Export] MeshInstance3D Ball2;
    [Export] MeshInstance3D RayMarker1;
    [Export] MeshInstance3D RayMarker2;
    [Export] MeshInstance3D RayMarker3;
    [Export] MeshInstance3D RayMarker4;


    [Export] MeshInstance3D RayOrigin1;
    [Export] MeshInstance3D RayOrigin2;
    [Export] MeshInstance3D RayOrigin3;
    [Export] MeshInstance3D RayOrigin4;



    [Export(PropertyHint.Range, "0,90")]
    public float TotalPitch { get; set; } = 85f;
    public Vector2 mouseMotion = Vector2.Zero;
	public float moveSpeed = 40.0f;


    [Export] float _NearPlane = 0.1f;
    [Export] float _FarPlane = 8;
    [Export] float FOV = 1;
    [Export] float AspectRatio = 1;

    Vector3 TestPosition = Vector3.Zero;
    Projection ViewMatrix = new Projection();
    Projection InvViewMatrix = new Projection();


    public byte ByteLerp(byte b, byte c, float t)
    {
        t = Mathf.Clamp(t, 0.0f, 1.0f);
        b = (byte)(b * (1.0 - t) + c * t);
        b = (byte)Mathf.Clamp(b, 0, 255);
        return b;
    }

    List<Image> Images = new List<Image>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

    public Image ProcessImages(List<Image> images)
    {
        byte[] AccumulatedData = images[0].GetData();


        List<byte[]> imageData = new List<byte[]>();
        foreach(Image image in images)
        {
            imageData.Add(image.GetData());
        }



        for (int p = 0; p < AccumulatedData.Length; p++)
        {
            int p_accumulator = 0;
            for (int i = 0; i < imageData.Count; i++)
            {
                p_accumulator += imageData[i][p];
            }
            AccumulatedData[p] = (byte)(p_accumulator / imageData.Count);
        }

        Image FinalImage = new Image();
        FinalImage.SetData(images[0].GetWidth(), images[0].GetHeight(), false, Image.Format.Rgb8, AccumulatedData);
        GD.Print($"{images.Count}");
        return FinalImage;
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

                    Projection RotationMatrix = XRotationMatrix * ZRotationMatrix * YRotationMatrix ;


                    Projection TranslationMatrix = new Projection();
                    TranslationMatrix.X = new Vector4(1, 0, 0, 0);
                    TranslationMatrix.Y = new Vector4(0, 1, 0, 0);
                    TranslationMatrix.Z = new Vector4(0, 0, 1, 0);
                    TranslationMatrix.W = new Vector4(-GlobalPosition.X, -GlobalPosition.Y, -GlobalPosition.Z, 1);


                    ViewMatrix = RotationMatrix * TranslationMatrix;
                    InvViewMatrix = (RotationMatrix * TranslationMatrix).Inverse();

                    Projection ProjectionMatrix = new Projection();

                    Projection p = this.GetCameraProjection();
                    
                    float FocalLength = 1.0f / Mathf.Tan(Mathf.DegToRad(FOV) / 2.0f);

                    float x = FocalLength / AspectRatio;
                    float y = -FocalLength;

                    float A = _NearPlane / (_FarPlane - _NearPlane);
                    float B = _FarPlane * A;

                    ProjectionMatrix.X = new Vector4(x, 0, 0, 0);
                    ProjectionMatrix.Y = new Vector4(0, y, 0, 0);
                    ProjectionMatrix.Z = new Vector4(0, 0, A, B);
                    ProjectionMatrix.W = new Vector4(0, 0, -1.0f, 0);


                    sm.SetShaderParameter("CameraPosition", Position);
                    sm.SetShaderParameter("CameraRotation", new Vector3(GlobalRotation.X, GlobalRotation.Y, 0));
                    sm.SetShaderParameter("ViewMatrix", ViewMatrix);
                    sm.SetShaderParameter("RotationViewMatrix", RotationMatrix);
                    sm.SetShaderParameter("InvViewMatrix", InvViewMatrix);
                    sm.SetShaderParameter("NearPlane", _NearPlane);
                    sm.SetShaderParameter("FarPlane", _FarPlane);
                    sm.SetShaderParameter("FOV", FOV);
                    sm.SetShaderParameter("AspectRatio", AspectRatio);
                    sm.SetShaderParameter("ProjectionMatrix", p);
                    sm.SetShaderParameter("InvProjectionMatrix", p.Inverse());
                    sm.SetShaderParameter("ViewTranslationMatrix", TranslationMatrix);
                    sm.SetShaderParameter("InvViewTranslationMatrix", TranslationMatrix.Inverse());

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

            if(Input.IsActionPressed("ui_select"))
            {
                Image m = GetViewport().GetTexture().GetImage();
                m.Convert(Image.Format.Rgb8);
                //m.SavePng($"res://renders/Render{count}.png");
                Images.Add(m);
                //count++;
            }


            if(Input.IsActionJustReleased("ui_select"))
            {

                byte test = 33;
                byte test2 = 255;
                GD.Print($"{ByteLerp(test,test2, 0.5f)}");

                Image render = ProcessImages(Images);

                render.SavePng($"res://renders/Render{Guid.NewGuid()}.png");
                Images.Clear();
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
