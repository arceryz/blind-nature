using Godot;
using System;
using Godot.Collections;

public partial class Player : CharacterBody3D
{
	public static Player Instance;

	[ExportGroup("SFX")]
	[Export] AudioStreamPlayer3D StepSFX;
	[Export] float StepSize = 0.5f;
	[Export] float StepFalloffTime = 1.0f;
	public float StepAccumulator = 0.0f;
	

	[ExportGroup("Properties")]
	[Export] Array<Node3D> ArcheryLocations;
	int CurrentLocation = 0;

	const float GRAVITY = 10.0f;
	const float MOUSE_LOOK_H = 1.0f;
	const float MOUSE_LOOK_V = 1.0f;
	const float MOVE_SPEED = 4.0f;

	Camera3D camera;

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		camera = GetNode<Camera3D>("Camera");
		Input.MouseMode = Input.MouseModeEnum.Captured;

		if (!ForestNetwork.Instance.IsNodeReady())
		{
			ForestNetwork.Instance.Ready += _NetworkReady;
		}
	}

	public void _NetworkReady()
	{
		Kestrel.Instance.NavigateTo(ArcheryLocations[0].GlobalPosition);
	}

	public override void _PhysicsProcess(double delta)
	{
		ProcessMovement((float)delta);
	}

	public override void _Input(InputEvent e)
	{
		if (e.IsActionPressed("free_camera"))
		{
			Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
		}
		if (e is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			Rotation = Rotation with { Y = Rotation.Y - motion.Relative.X * MOUSE_LOOK_V / 360.0f };
			camera.Rotation = camera.Rotation with
			{
				X = Mathf.Clamp(camera.Rotation.X - motion.Relative.Y * MOUSE_LOOK_H / 640.0f, -Mathf.Pi/2, Mathf.Pi/2)
			};
		}
		if (e.IsActionPressed("blind_mode"))
		{
			// Should make this a GUI toggle!
			if (camera.Current)
				camera.ClearCurrent(false);
			else
				camera.MakeCurrent();
		}

		if (e.IsActionPressed("ui_accept"))
		{
			CurrentLocation = (CurrentLocation + 1) % ArcheryLocations.Count;
			Kestrel.Instance.NavigateTo(ArcheryLocations[CurrentLocation].GlobalPosition);
		}
	}

	void ProcessMovement(float delta)
	{
		// Get direction from input.
		Vector2 input = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Vector3 direction = camera.GlobalBasis * new Vector3(input.X, 0, input.Y);
		direction.Y = 0;
		direction = direction.Normalized();
		
		// Set velocity vector.
		float speed = MOVE_SPEED * (Input.IsActionPressed("debug_sprint") ? 5.0f: 1.0f);
		Vector3 velocity = Velocity;
		velocity.X = direction.X * speed;
		velocity.Z = direction.Z * speed;

		if (input.IsZeroApprox())
		{
			StepAccumulator -= StepFalloffTime * StepSize * delta;
			StepAccumulator = Mathf.Max(StepAccumulator, 0.0f);
		}
		else
		{
			StepAccumulator += speed * delta;
		}

		if (StepAccumulator > StepSize)
		{
			StepSFX.Play();
			Vibration.Instance.PlayStep(1.0f);
			StepAccumulator = 0.0f;
		}

		// Gravity.
			if (!IsOnFloor())
			{
				velocity.Y -= GRAVITY * delta;
			}
			else
			{
				velocity.Y = 0;
			}

		Velocity = velocity;
		MoveAndSlide();
	}
}
