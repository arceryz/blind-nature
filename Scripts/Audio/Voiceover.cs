using Godot;
using System;

[GlobalClass]
public partial class Voiceover : Node3D
{
	public static Voiceover GlobalVoice;

	[Export] bool IsGlobalVoice = false;
	[Export] int FadeTimeMs = 0;
	[Export] int InterpolationMs = 0;

	int activeId = -1;

	public override void _EnterTree()
	{
		if (IsGlobalVoice) GlobalVoice = this;
	}

	public override void _Ready()
	{
	}

	public void Play(string voiceEvent)
	{
		if (activeId > 0) Stop();
		activeId = WwiseSharp.Instance.PostEvent(voiceEvent, this);
	}

	public void Stop()
	{
		WwiseSharp.Instance.StopEvent(activeId, FadeTimeMs, InterpolationMs);
	}
}
