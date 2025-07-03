using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class LioraBow : Node3D
{
	[Signal] public delegate void TargetHitEventHandler();
	[Signal] public delegate void BowDrawEventHandler();
	[Signal] public delegate void ArrowReleaseEventHandler();
	[Signal] public delegate void AimTargetEnteredEventHandler();
	[Signal] public delegate void AimTargetExitedEventHandler();

	[ExportGroup("Properties")]
	[Export] Camera3D Camera;
	ColorRect AimVision;
	AimTarget AimTarget;

	// Estimate aiming direction.
	Vector2 AimDirection = Vector2.Zero;
	Vector2[] ScreenSamples = new Vector2[10];
	int SampleIndex = 0;
	Vector2 TargetScreenPrev = Vector2.Zero;

	Vector2 TargetScreen = Vector2.Zero;
	Vector2 TargetScreenCentered = Vector2.Zero;
	float TargetScreenRadius = 0.0f;
	bool TargetVisible = false;
	bool IsOnTarget = false;
	public float AimProximity = 0.0f;

	QuickTimer DrawDelayTimer;

	int aimOnTargetId;

	bool IsAiming = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		DrawDelayTimer = new(this, null, 1.0f, 0.0f, true, false);
		
		AimVision = new ColorRect();
		AimVision.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		AddChild(AimVision);
		AimVision.Draw += _OnAimVisionQueueRedraw;
	}

	public void DrawString()
	{
		if (!DrawDelayTimer.IsFinished()) return;
		IsAiming = true;
		WwiseSharp.Instance.PostEvent("play_bow_draw", this);
	}

	public void ReleaseArrow()
	{
		IsAiming = false;
		DrawDelayTimer.Start();
		WwiseSharp.Instance.PostEvent("play_bow_arrow_release", this);
		WwiseSharp.Instance.StopEvent(aimOnTargetId, 0, 0);

		if (IsOnTarget)
		{
			EmitSignal(SignalName.TargetHit);
			WwiseSharp.Instance.PostEvent("play_bow_target_hit", this);
		}
	}

	public void CancelArrow()
	{
		IsAiming = false;
	}

	public void PickClosestTarget()
	{
		Array<Node> targets = GetTree().GetNodesInGroup("aim_target");
		float closestDist = -1;
		AimTarget closest = null;
		foreach (AimTarget mark in targets)
		{
			float dist = mark.GlobalPosition.DistanceSquaredTo(GlobalPosition);
			if (dist < closestDist || closestDist == -1)
			{
				closestDist = dist;
				closest = mark;
			}
		}
		if (closest != null)
		{
			SetAimTarget(closest);
		}
	}

	public void SetAimTarget(AimTarget target)
	{
		Debug.Log(Class.LioraBow, String.Format("Targeting {0}", target.Name));
		AimTarget = target;
		AimTarget.Start();
	}

	public void StopTargeting()
	{
		AimTarget.Stop();
	}

	public override void _Process(double delta)
	{
		if (IsAiming)
		{
			AimVision.Color = Colors.Black with { A = 0.5f };
			AimVision.Show();
			ProcessTargetScreen();
			AimVision.QueueRedraw();
			if (TargetVisible)
			{
				Haptics.Instance.PlayDirectionalFeedback(AimDirection, TargetScreenCentered, TargetScreenRadius);
			}
			else
			{
			}
		}
		else
		{
			AimVision.Hide();
		}
	}

	void ProcessTargetScreen()
	{
		// Average the samples for the aim direction.
		// Not sure if this is the best approach.
		ScreenSamples[SampleIndex] = TargetScreenPrev - TargetScreen;
		TargetScreenPrev = TargetScreen;
		SampleIndex = (SampleIndex + 1) % ScreenSamples.Length;

		AimDirection = Vector2.Zero;
		foreach (Vector2 sample in ScreenSamples)
		{
			AimDirection += sample;
		}
		AimDirection = AimDirection.Normalized();

		// Compute the target's position in clip-space coordinates.
		Rect2 screen = GetViewport().GetVisibleRect();

		// Multiply by view transform first. Camera transform is the inverse of the view transform.
		// Then multiply by the projection matrix with W=1.0 (default homogenous coords non-infinity).
		// Divide out the depth component to get the clip coordinates.
		// Finally map the clip coordinates to screen coordinates, keeping into account that the
		// default coordinate system for has -1 at the top, as opposed to 1 for OpenGL standards.
		// Thus multiply Y by -1.
		Projection projMatrix = Camera.GetCameraProjection();
		Vector3 targetView = Camera.GetCameraTransform().Inverse() * AimTarget.GetTargetPosition();
		Vector4 targetClip = projMatrix * new Vector4(targetView.X, targetView.Y, targetView.Z, 1.0f);
		Vector4 edgeClip = projMatrix * new Vector4(targetView.X + AimTarget.GetTargetRadius(), targetView.Y, targetView.Z, 1.0f);
		targetClip /= targetClip.W;
		edgeClip /= edgeClip.W;

		TargetScreen = new Vector2((targetClip.X * 0.5f + 0.5f) * screen.Size.X, (-targetClip.Y * 0.5f + 0.5f) * screen.Size.Y);
		TargetScreenCentered = TargetScreen - 0.5f * screen.Size;
		Vector2 edgeScreen = new Vector2((edgeClip.X * 0.5f + 0.5f) * screen.Size.X, (-edgeClip.Y * 0.5f + 0.5f) * screen.Size.Y);

		TargetScreenRadius = TargetScreen.DistanceTo(edgeScreen);
		AimProximity = targetView.AngleTo(Vector3.Forward) / Mathf.Pi;
		TargetVisible = targetView.Z < 0.0f;

		bool nowOnTarget = TargetScreenCentered.Length() < TargetScreenRadius && TargetVisible;
		if (nowOnTarget && !IsOnTarget) aimOnTargetId = WwiseSharp.Instance.PostEvent("play_bow_aim_on_target_loop", this);
		if (!nowOnTarget && IsOnTarget) WwiseSharp.Instance.StopEvent(aimOnTargetId);
		IsOnTarget = nowOnTarget;

		Debug.Log(Class.LioraBow, String.Format("screen={0}, radius={1}, aimdist={2}, visible={3}, dir={4}", TargetScreen, TargetScreenRadius, AimProximity, TargetVisible, AimDirection));
	}

	void _OnAimVisionQueueRedraw()
	{
		if (AimTarget != null)
		{
			Rect2 screen = GetViewport().GetVisibleRect();
			Vector2 center = screen.GetCenter();
			AimVision.DrawCircle(center, 8.0f, Colors.Red, false);
			AimVision.DrawLine(center, center + AimDirection * 50.0f, Colors.White);
			if (TargetVisible) AimVision.DrawCircle(TargetScreen, 8.0f, Colors.Green, false);
			if (TargetVisible) AimVision.DrawCircle(TargetScreen, TargetScreenRadius, Colors.Green, false);
		}
	}

	public bool CanRelease()
	{
		return IsAiming;
	}

	public bool CanDraw()
	{
		return DrawDelayTimer.IsFinished();
	}
}
