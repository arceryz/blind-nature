using Godot;
using System;
using System.Collections.Generic;

public partial class FloorSfx : Node3D
{
	[Export] FloorDetector FloorDetector;

	public void Play()
	{
		var audio = FindChild(FloorDetector.FloorType) as AudioStreamPlayer3D;
		audio.Play();
	}
}
