using Godot;
using System;

public partial class AimTarget : StaticBody3D
{
	CollisionShape3D TargetShape;
	float Radius;
	int eventId;

	public override void _EnterTree()
	{
		// A bit of a hack for now.
		AddToGroup("aim_target");
	}

	public override void _Ready()
	{
		// TargetShape must be a sphere to give the radius for the aiming system.
		TargetShape = GetNode<CollisionShape3D>("TargetShape");
		SphereShape3D sphere = (SphereShape3D)TargetShape.Shape;
		Radius = sphere.Radius;
	}

	public float GetTargetRadius()
	{
		return Radius;
	}

	public Vector3 GetTargetPosition()
	{
		return TargetShape.GlobalPosition;
	}

	public void Start()
	{
		eventId = WwiseSharp.Instance.PostEvent("play_practice_target_loop", this);
	}

	public void Stop()
	{
		WwiseSharp.Instance.StopEvent(eventId);
	}
}
