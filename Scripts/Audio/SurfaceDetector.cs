using Godot;
using System;

public enum SurfaceType
{
	None,
	Dirt,
	Puddle,
}

[GlobalClass]
public partial class SurfaceDetector : RayCast3D
{
	public SurfaceType CurrentSurface;

	public override void _Process(double delta)
	{
		Node collider = GetCollider() as Node;
		if (collider != null)
		{
			SurfaceType newType = SurfaceType.None;
			if (collider.IsInGroup("surface_dirt")) newType = SurfaceType.Dirt;
			if (collider.IsInGroup("surface_puddle")) newType = SurfaceType.Puddle;
			if (River.Instance.IsPlayerOnWater()) newType = SurfaceType.Puddle;

			if (CurrentSurface != newType)
			{
				CurrentSurface = newType;
				Debug.Log(Class.SurfaceDetector, $"Surface changed to: {newType}");
			}
		}
	}
}
