using Godot;
using System;

public partial class CameraToggle : Camera3D
{
	public override void _Ready()
	{
	}

	public override void _Input(InputEvent ev)
	{
		if (ev.IsActionPressed("camera_toggle"))
		{
			Current = !Current;
		}
	}

}
