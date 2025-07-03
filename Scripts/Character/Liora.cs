using Godot;
using System;
using Godot.Collections;
using System.Runtime.CompilerServices;

public partial class Liora : CharacterBody3D
{
	public static Liora Instance;

	[ExportGroup("Steps")]
	[Export] SurfaceDetector surfaceDetector;
	[Export] float StepSize = 0.5f;
	[Export] float StepFalloffTime = 1.0f;
	public float StepAccumulator = 0.0f;

	[ExportGroup("Movement")]
	[Export] float MouseLookH = 1.0f;
	[Export] float MouseLookV = 1.0f;
	[Export] float MoveSpeed = 4.0f;
	[Export] float ControllerTurnSpeed = 1.0f;
	[Export] float ControllerAimSpeedUp = 1.0f;
	[Export(PropertyHint.Range, "0, 90")] float AimAngleLimit = 45.0f;

	[Export] Array<Node3D> ArcheryLocations;
	LioraBow Bow;
	Camera3D Camera;
	int CurrentLocation = 0;
	float AimSpeed = 1.0f;
	Vector3 Direction = Vector3.Zero;

	enum State
	{
		Walking,
		Aiming
	}
	State CurrentState = State.Walking;

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		Bow = GetNode<LioraBow>("%LioraBow");
		Camera = GetNode<Camera3D>("Camera");
		Input.MouseMode = Input.MouseModeEnum.Captured;

		if (!ForestNetwork.Instance.IsNodeReady())
		{
			ForestNetwork.Instance.Ready += _NetworkReady;
		}
		Bow.TargetHit += _OnArrowHitTarget;
		Kestrel.Instance.NavigationFinished += _OnKestrelNavigationFinished;
	}

	public void _NetworkReady()
	{
		Kestrel.Instance.NavigateTo(ArcheryLocations[0].GlobalPosition);
	}

	public void _OnArrowHitTarget()
	{
		CurrentLocation = (CurrentLocation + 1) % ArcheryLocations.Count;
		Kestrel.Instance.NavigateTo(ArcheryLocations[CurrentLocation].GlobalPosition);
		Bow.StopTargeting();
	}

	public void _OnKestrelNavigationFinished()
	{
		Bow.PickClosestTarget();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (UserInterface.Instance.IsMenuActive()) return;
		switch (CurrentState)
		{
			case State.Walking:
				AimSpeed = 1.0f;
				Camera.Rotation = Camera.Rotation with { X = Mathf.Lerp(Camera.Rotation.X, 0.0f, (float)delta * 10.0f) };
				//ProcessRouteDirectionFeedback((float)delta);
				ProcessMovement((float)delta, MoveSpeed);
				break;

			case State.Aiming:
				ProcessMovement((float)delta, 0.3f * MoveSpeed);
				ProcessAiming((float)delta);
				break;
		}

		if (Input.IsActionJustPressed("draw_string") && Bow.CanDraw() && !Kestrel.Instance.IsNavigating())
		{
			CurrentState = State.Aiming;
			Bow.DrawString();
		}
		if (Input.IsActionJustReleased("draw_string") && Bow.CanRelease())
		{
			CurrentState = State.Walking;
			Bow.ReleaseArrow();
		}
	}

	public override void _Input(InputEvent e)
	{
		if (e is InputEventMouseMotion motion && !UserInterface.Instance.IsMenuActive())
		{
			Rotation = Rotation with { Y = Rotation.Y - motion.Relative.X * MouseLookV / 360.0f };
			Camera.Rotation = Camera.Rotation with
			{
				X = Mathf.Clamp(Camera.Rotation.X - motion.Relative.Y * MouseLookH / 640.0f, -Mathf.Pi / 2, Mathf.Pi / 2)
			};
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
			Haptics.Instance.PlayDirectionalFeedback(direction, target, Kestrel.Instance.GetVibrationFeedbackRadius());
		}
	}

	void ProcessMovement(float delta, float baseSpeed)
	{
		// Get direction from input.
		Vector2 input = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Direction = Camera.GlobalBasis * new Vector3(input.X, 0, input.Y);
		Direction.Y = 0;
		Direction = Direction.Normalized();

		// Rotation with controller.
		float axis = Input.GetAxis("turn_left", "turn_right");
		float rotDelta = axis * ControllerTurnSpeed * delta * AimSpeed;
		Rotation = Rotation with { Y = Rotation.Y - rotDelta };
		Haptics.Instance.PlayRotationFeedback(rotDelta);

		// Set velocity vector.
		float speed = baseSpeed * (Input.IsActionPressed("debug_sprint") ? 5.0f : 1.0f);
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
			WwiseSharp.Instance.SetSwitch("surface_type", surfaceDetector.CurrentSurface.ToString(), this);
			WwiseSharp.Instance.PostEvent("play_footstep", this);
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

	void ProcessAiming(float delta)
	{
		AimSpeed = Mathf.Lerp(0.1f, 1.0f, Mathf.Clamp(Bow.AimProximity / 0.2f, 0.0f, 1.0f));
		float axis = Input.GetAxis("aim_down", "aim_up");
		float limit = Mathf.DegToRad(AimAngleLimit);
		Camera.Rotation = Camera.Rotation with
		{
			X = Mathf.Clamp(Camera.Rotation.X + axis * ControllerAimSpeedUp * delta * AimSpeed, -limit, limit)
		};
	}
}
