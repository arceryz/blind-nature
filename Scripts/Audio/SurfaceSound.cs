using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class SurfaceSound : Node3D
{
	[Export] SurfaceDetector SurfaceDetector;
	
	[ExportGroup("Surfaces")]
	[Export] string DirtEvent;
	[Export] string WaterEvent;

	public void Play()
	{
		string ev = "";
		switch (SurfaceDetector.CurrentSurface)
		{
			case SurfaceType.Dirt: ev = DirtEvent; break;
			case SurfaceType.Puddle: ev = WaterEvent; break;
		}
		if (ev != "") WwiseSharp.Instance.PostEvent(ev, this);
	}
}
