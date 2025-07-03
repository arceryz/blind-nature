using Godot;
using System;

public partial class RepeatSfx : AudioStreamPlayer3D
{
	[Export] float Interval = 1.0f;
	[Export] float IntervalRandom = 1.0f;

	QuickTimer Timer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Timer = new QuickTimer(this, () => Play(), Interval, IntervalRandom, false, true);
	}

	public void StartRepeating() => Timer.Start();
	public void StopRepeating() => Timer.Stop();
}
