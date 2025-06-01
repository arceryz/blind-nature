using Godot;
using System;
using Godot.Collections;
using System.Runtime.CompilerServices;

public partial class Player : CharacterBody3D
{
	public static Player Instance;

	[ExportGroup("SFX")]
	[Export] AudioStreamPlayer3D StepSFX;
	[Export] float StepSize = 0.5f;
	[Export] float StepFalloffTime = 1.0f;
	public float StepAccumulator = 0.0f;
	

	[ExportGroup("Movement")]
	[Export] float mouse_look_h = 1.0f;
	[Export] float mouse_look_v = 1.0f;
	[Export] float move_speed = 4.0f;
	[Export] float controller_turn_speed = 1.0f;

	[Export] Array<Node3D> ArcheryLocations;
	Camera3D camera;
	int CurrentLocation = 0;


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
			Rotation = Rotation with { Y = Rotation.Y - motion.Relative.X * mouse_look_v / 360.0f };
			camera.Rotation = camera.Rotation with
			{
				X = Mathf.Clamp(camera.Rotation.X - motion.Relative.Y * mouse_look_h / 640.0f, -Mathf.Pi/2, Mathf.Pi/2)
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

		// Rotation with controller.
		float axis = Input.GetAxis("turn_left", "turn_right");
		Rotation = Rotation with { Y = Rotation.Y - axis * controller_turn_speed * delta };
		
		// Set velocity vector.
		float speed = move_speed * (Input.IsActionPressed("debug_sprint") ? 5.0f: 1.0f);
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
				velocity.Y -= 10.0f * delta;
			}
			else
			{
				velocity.Y = 0;
			}

		Velocity = velocity;
		MoveAndSlide();
	}
}
