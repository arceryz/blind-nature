using Godot;
using System;

public partial class River : Path3D
{
	public static River Instance;

	[Export] Node3D RiverSfx;
	[Export] float RiverWidth = 3.0f;
	bool PlayerNearRiver = false;
	Vector3 RiverPoint = Vector3.Zero;
	Vector3 TargetRiverPoint = Vector3.Zero;
	bool FirstTime = true;

	public override void _EnterTree()
	{
		Instance = this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector3 lioraGlobal = Liora.Instance.GlobalPosition + Vector3.Up * 2.0f;
		TargetRiverPoint = ToGlobal(Curve.GetClosestPoint(ToLocal(lioraGlobal)));
		TargetRiverPoint = TargetRiverPoint.MoveToward(lioraGlobal, RiverWidth);
		RiverPoint = RiverPoint.Lerp(TargetRiverPoint, 5.0f * (float)delta);
		if (FirstTime || PlayerNearRiver)
		{
			RiverPoint = TargetRiverPoint;
			FirstTime = false;
		}

		float dist = RiverPoint.DistanceTo(lioraGlobal);
		RiverSfx.GlobalPosition = RiverPoint;

		bool IsNear = dist < RiverWidth * 0.5f;
		PlayerNearRiver = IsNear;
	}

	public bool IsPlayerOnWater()
	{
		return PlayerNearRiver;
	}
}
