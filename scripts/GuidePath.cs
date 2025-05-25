using Godot;
using System;
using System.Dynamic;

[Tool]
[GlobalClass]
public partial class GuidePath : Path3D
{
	[Export]
	public Node3D Junction1
	{
		get { return _Junction1; }
		set
		{
			_Junction1 = value;
			Update();
		}
	}
	Node3D _Junction1;

	[Export]
	public Node3D Junction2
	{
		get { return _Junction2; }
		set
		{
			_Junction2 = value;
			Update();
		}
	}
	Node3D _Junction2;

	public override void _Ready()
	{
		Update();
	}

	void Update()
	{
		if (!IsNodeReady()) return;
		if (Junction1 != null) Curve.SetPointPosition(0, ToLocal(Junction1.GlobalPosition)+Vector3.Up);
		if (Junction2 != null) Curve.SetPointPosition(Curve.PointCount-1, ToLocal(Junction2.GlobalPosition)+Vector3.Up);
		if (Junction1 != null && Junction2 != null)
		{
			Name = String.Format("{0} <> {1}", Junction1.Name, Junction2.Name);
		}
	}
}
