using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class Kestrel : Node3D
{
	public static Kestrel Instance;

	[Signal] public delegate void NavigationFinishedEventHandler();

	[ExportGroup("Navigation")]
	[Export] float CallingInterval = 1.0f;
	[Export] float CallingIntervalRandom = 1.0f;
	[Export] float CallingDistanceMin = 5.0f;
	[Export] float CallingDistanceMax = 15.0f;
	[Export] float RelocateDistanceForward = 10.0f;
	[Export] float RelocateDistanceBackward = 10.0f;
	[Export] float RelocateSpeed = 1.0f;
	[Export] float RelocateHeight = 1.0f;
	[Export] float PlayerArrivalDistance = 1.0f;

	enum State
	{
		Calling,
		Relocating,
		Idle
	}
	State CurrentState = State.Idle;
	Label3D StateLabel;
	ForestRoute Route;

	float PlayerTargetDistance = 0.0f;
	float PlayerDistance = 0.0f;
	float PlayerPathDistance = 0.0f;
	float PlayerPathOffset = 0.0f;

	Vector3 TargetLocation = Vector3.Zero;
	float TargetOffset = 0.0f;
	float CurrentOffset = 0.0f;

	float CallingTimerDuration = 0.0f;
	float CallingTimer = 0.0f;

	int flappingId;
	int callingId;

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		StateLabel = GetNode<Label3D>("StateLabel");
	}

	public override void _Process(double delta)
	{
		StateLabel.Text = CurrentState.ToString();

		switch (CurrentState)
		{
			case State.Calling:
				PlayerDistance = Liora.Instance.GlobalPosition.DistanceTo(GlobalPosition);
				PlayerPathOffset = Route.GetClosestOffset(Liora.Instance.GlobalPosition);
				PlayerPathDistance = Route.Sample(PlayerPathOffset).DistanceTo(Liora.Instance.GlobalPosition);
				PlayerTargetDistance = Liora.Instance.GlobalPosition.DistanceTo(TargetLocation);

				// We switch to idle if the player has arrived.
				if (PlayerTargetDistance < PlayerArrivalDistance)
				{
					CurrentState = State.Idle;
					EmitSignal(SignalName.NavigationFinished);
					WwiseSharp.Instance.StopEvent(callingId, 500, 0);
					break;
				}

				// Everytime we call, we also recalculate the route if too far.
				// This avoids spamming the recalculate.
				if (CallingTimer > CallingTimerDuration)
				{
					CallingTimer = 0.0f;
					CallingTimerDuration = CallingInterval + GD.Randf() * CallingIntervalRandom;

					if (PlayerPathDistance > CallingDistanceMax)
					{
						RecalculateRoute();
					}
				}
				CallingTimer += (float)delta;

				// Relocate if too close or too far.
				if (PlayerDistance < CallingDistanceMin || PlayerDistance > CallingDistanceMax)
				{
					RelocateToGuidingPosition();
				}
				break;

			case State.Relocating:
				// Code runs when in flight.
				break;

			case State.Idle:
				break;
		}
	}

	void _OnRelocatingEnd()
	{
		CurrentState = State.Calling;
		WwiseSharp.Instance.StopEvent(flappingId, 0, 0);
		callingId = WwiseSharp.Instance.PostEvent("play_kestrel_calling_loop", this);
	}

	void _OnRelocatingStart()
	{
		flappingId = WwiseSharp.Instance.PostEvent("play_kestrel_flying", this);
		WwiseSharp.Instance.StopEvent(callingId, 0, 0);
	}

	public void RelocateToGuidingPosition()
	{
		float reloc = PlayerDistance < CallingDistanceMin ? RelocateDistanceForward : RelocateDistanceBackward;
		float newOffset = PlayerPathOffset + Mathf.Max(reloc - PlayerPathDistance, 0.0f);
		newOffset = Mathf.Min(newOffset, TargetOffset);
		Vector3 newPosition = Route.Sample(newOffset);

		// No point in relocating if too close together.
		if (Mathf.Abs(CurrentOffset - newOffset) < 0.01f)
		{
			return;
		}

		CallingTimerDuration = 0.0f;
		CurrentState = State.Relocating;
		CurrentOffset = newOffset;

		_OnRelocatingStart();
		Tween tw = CreateTween();
		tw.TweenProperty(this, "global_position", (GlobalPosition + newPosition) * 0.5f + Vector3.Up * RelocateHeight, RelocateSpeed / 2.0);
		tw.TweenProperty(this, "global_position", newPosition, RelocateSpeed / 2.0f);
		tw.TweenCallback(Callable.From(_OnRelocatingEnd));
	}

	public void RecalculateRoute()
	{
		Route = ForestNetwork.Instance.GetClosestPathRoute(Liora.Instance.GlobalPosition, TargetLocation);
		TargetOffset = Route.GetClosestOffset(TargetLocation);
		CurrentState = State.Calling;
		WwiseSharp.Instance.StopEvent(callingId);
		callingId = WwiseSharp.Instance.PostEvent("play_kestrel_calling_loop", this);
		//NavDebugAgent.Instance.SetRoute(Route);
	}

	public void NavigateTo(Vector3 location)
	{
		TargetLocation = location;
		CallDeferred(MethodName.RecalculateRoute);
	}

	public bool IsNavigating()
	{
		return Route != null && CurrentState != State.Idle;
	}

	public float GetVibrationFeedbackRadius()
	{
		return 1.0f;
	}
}
