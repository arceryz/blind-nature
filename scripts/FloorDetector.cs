using Godot;
using System;

public partial class FloorDetector : RayCast3D
{
	// We have to use strings because the floor type is set as metadata in the editor.
	// Ideally we use enums, but strings are inevitable anyway.
	// A class on each of the floor objects is a bit excessive.
	[Signal] public delegate void FloorChangedEventHandler(StringName floorType);
	public StringName FloorType = "Ground";

	public override void _Process(double delta)
	{
		GodotObject collider = GetCollider();
		if (collider != null)
		{
			StringName type = collider.GetMeta("FloorType", "Ground").AsStringName();
			if (River.Instance.IsPlayerOnWater()) type = "Puddle";
			if (type != FloorType)
			{
				FloorType = type;
				EmitSignal(SignalName.FloorChanged, FloorType);
				GD.Print($"Floor changed to: {FloorType}");
			}
		}
	}
}
