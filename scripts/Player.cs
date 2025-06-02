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
	[Export] float MouseLookH = 1.0f;
	[Export] float MouseLookV = 1.0f;
	[Export] float MoveSpeed = 4.0f;
	[Export] float ControllerTurnSpeed = 1.0f;

	[Export] Array<Node3D> ArcheryLocations;
	Camera3D camera;
	int CurrentLocation = 0;
	Vector3 Direction = Vector3.Zero;

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
		ProcessRouteDirectionFeedback((float)delta);
	}

	public override void _Input(InputEvent e)
	{
		if (e.IsActionPressed("free_camera"))
		{
			Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
		}
		if (e is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			Rotation = Rotation with { Y = Rotation.Y - motion.Relative.X * MouseLookV / 360.0f };
			camera.Rotation = camera.Rotation with
			{
				X = Mathf.Clamp(camera.Rotation.X - motion.Relative.Y * MouseLookH / 640.0f, -Mathf.Pi/2, Mathf.Pi/2)
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

	void ProcessRouteDirectionFeedback(float delta)
	{
		if (Kestrel.Instance.IsNavigating())
		{
			Vector2 directionView = -new Vector2(GlobalBasis.Z.X, GlobalBasis.Z.Z).Normalized();
			Vector2 direction = new Vector2(Direction.X, Direction.Z).Normalized();
			Vector3 target3 = Kestrel.Instance.GlobalPosition - GlobalPosition;
			Vector2 target = new Vector2(target3.X, target3.Z);
			Vibration.Instance.PlayDirectionalFeedback(direction, target, Kestrel.Instance.GetVibrationFeedbackRadius());
		}
	}

	void ProcessMovement(float delta)
	{
		// Get direction from input.
		Vector2 input = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Direction = camera.GlobalBasis * new Vector3(input.X, 0, input.Y);
		Direction.Y = 0;
		Direction = Direction.Normalized();

		// Rotation with controller.
		float axis = Input.GetAxis("turn_left", "turn_right");
		Rotation = Rotation with { Y = Rotation.Y - axis * ControllerTurnSpeed * delta };
		
		// Set velocity vector.
		float speed = MoveSpeed * (Input.IsActionPressed("debug_sprint") ? 5.0f: 1.0f);
		Vector3 velocity = Velocity;
		velocity.X = Direction.X * speed;
		velocity.Z = Direction.Z * speed;

		if (input.IsZeroApprox())
		{
			StepAccumulator -= StepFalloffTime * StepSize * delta;
			StepAccumulator = Mathf.Max(StepAccumulator, 0.0f);
		}
		else
		{
			Vector3 hvel = GetRealVelocity();
			hvel.Y = 0;
			StepAccumulator += hvel.Length() * delta;
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
