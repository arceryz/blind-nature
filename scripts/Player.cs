using Godot;
using System;
using Godot.Collections;

public partial class Player : CharacterBody3D
{
	public static Player Instance;

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
			Vector3 rot = camera.Rotation;
			rot.X -= motion.Relative.Y * MOUSE_LOOK_H / 640.0f;
			rot.Y -= motion.Relative.X * MOUSE_LOOK_V / 360.0f;
			camera.Rotation = rot;
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
		// Regular movement.
		Vector2 input = Input.GetVector("move_left", "move_right", "move_up", "move_down");

		Vector3 worldMovement = camera.GlobalBasis * new Vector3(input.X, 0, input.Y);
		worldMovement.Y = 0;
		worldMovement = worldMovement.Normalized() * MOVE_SPEED;

		Vector3 velocity = Velocity;
		velocity.X = worldMovement.X;
		velocity.Z = worldMovement.Z;

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
