using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class Kestrel : Node3D
{
	public static Kestrel Instance;

	[ExportGroup("Navigation")]
	[Export] AudioStreamPlayer3D CallingSFX;
	[Export] float CallingInterval = 1.0f;
	[Export] float CallingIntervalRandom = 1.0f;
	[Export] float CallingDistanceMin = 5.0f;
	[Export] float CallingDistanceMax = 15.0f;
	[Export] float RelocateDistanceForward = 10.0f;
	[Export] float RelocateDistanceBackward = 10.0f;
	[Export] float RelocateSpeed = 1.0f;
	[Export] float RelocateHeight = 1.0f;

	enum State
	{
		Calling,
		Relocating,
		Idle
	}
	State CurrentState = State.Idle;
	Label3D StateLabel;
	ForestRoute Route;

	float PlayerDistance = 0.0f;
	float PlayerPathDistance = 0.0f;
	float PlayerPathOffset = 0.0f;

	Vector3 TargetLocation = Vector3.Zero;
	float TargetOffset = 0.0f;
	float CurrentOffset = 0.0f;

	float CallingTimerDuration = 0.0f;
	float CallingTimer = 0.0f;

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
		PlayerDistance = Player.Instance.GlobalPosition.DistanceTo(GlobalPosition);

		if (CurrentState == State.Calling)
		{
			if (CurrentOffset == TargetOffset)
			{
				CurrentState = State.Idle;
				return;
			}

			NavDebugAgent.Instance.SetRoute(Route);
			if (CallingTimer > CallingTimerDuration)
			{
				CallingTimer = 0.0f;
				CallingTimerDuration = CallingInterval + GD.Randf() * CallingIntervalRandom;
				CallingSFX.Play();

				if (PlayerPathDistance > CallingDistanceMax)
				{
					RecalculateRoute();
				}
			}
			CallingTimer += (float)delta;

			if (PlayerDistance < CallingDistanceMin || PlayerDistance > CallingDistanceMax)
			{
				RelocateToGuidingPosition();
			}
		}

			else if (CurrentState == State.Relocating)
			{

			}
	}

	public void RelocateToGuidingPosition()
	{
		PlayerPathOffset = Route.GetClosestOffset(Player.Instance.GlobalPosition);
		PlayerPathDistance = Route.Sample(PlayerPathOffset).DistanceTo(Player.Instance.GlobalPosition);

		if (PlayerPathDistance > CallingDistanceMax)
		{
			return;
		}

		float reloc = PlayerDistance < CallingDistanceMin ? RelocateDistanceForward : RelocateDistanceBackward;
		CurrentOffset = PlayerPathOffset + Mathf.Max(reloc - PlayerPathDistance, 0.0f);
		CurrentOffset = Mathf.Min(CurrentOffset, TargetOffset);
		Vector3 newPosition = Route.Sample(CurrentOffset);

		CallingTimerDuration = 0.0f;
		CurrentState = State.Relocating;

		Tween tw = CreateTween();
		tw.TweenProperty(this, "global_position", (GlobalPosition + newPosition) * 0.5f + Vector3.Up * RelocateHeight, RelocateSpeed / 2.0);
		tw.TweenProperty(this, "global_position", newPosition, RelocateSpeed / 2.0f);
		tw.TweenProperty(this, "CurrentState", (int)State.Calling, 0.0f);
	}

	public void RecalculateRoute()
	{
		Route = ForestNetwork.Instance.GetClosestPathRoute(Player.Instance.GlobalPosition, TargetLocation);
		TargetOffset = Route.GetClosestOffset(TargetLocation);
		RelocateToGuidingPosition();
	}

	public void NavigateTo(Vector3 location)
	{
		TargetLocation = location;
		RecalculateRoute();
	}

}
