using Godot;
using System;
using System.Dynamic;

[Tool]
[GlobalClass]
public partial class GuidePath : Path3D
{
	[Export]	public Junction Junction1
	{
		get { return _Junction1; }
		set
		{
			_Junction1 = value;
			Update();
		}
	}
	Junction _Junction1;

	[Export]
	public Junction Junction2
	{
		get { return _Junction2; }
		set
		{
			_Junction2 = value;
			Update();
		}
	}
	Junction _Junction2;

	public override void _Ready()
	{
		Update();
	}

	public float GetLength()
	{
		return Curve.GetBakedLength();
	}

	public float GetDistance(Vector3 from)
	{
		return Curve.GetClosestPoint(GlobalPosition + from).DistanceTo(from);
	}

	public (Vector3, float) GetClosestPoint(Vector3 from)
	{
		float offset = Curve.GetClosestOffset(from + GlobalPosition);
		Vector3 point = Curve.SampleBaked(offset);
		return (point, offset);
	}

	public Vector3 Sample(float offset)
	{
		return Curve.SampleBaked(offset, true);
	}

	void Update()
	{
		if (!IsNodeReady()) return;
		if (Junction1 != null) Curve.SetPointPosition(0, ToLocal(Junction1.GlobalPosition) + Vector3.Up);
		if (Junction2 != null) Curve.SetPointPosition(Curve.PointCount - 1, ToLocal(Junction2.GlobalPosition) + Vector3.Up);
		if (Junction1 != null && Junction2 != null)
		{
			Name = String.Format("{0} <> {1}", Junction1.Name, Junction2.Name);
		}
	}
}
