using Godot;
using System;

public partial class AudioController : Node
{
	float MasterVolumeTarget = 1.0f;
	float MasterVolume = 1.0f;

	public override void _Ready()
	{

	}

	public override void _Process(double delta)
	{
		MasterVolume = Mathf.Lerp(MasterVolume, MasterVolumeTarget, (float)delta * 10.0f);
		AudioServer.SetBusVolumeLinear(0, MasterVolume);
	}


	public void SetMasterVolumePercent(float percent)
	{
		MasterVolumeTarget = percent / 100.0f;
	}
}
