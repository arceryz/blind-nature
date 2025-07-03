using Godot;
using System;

public partial class AcSlider : HSlider
{
	[Export] public string AnnounceEvent;
	int eventId;

	static string[] percentEvents = {
		"play_20_percent",
		"play_40_percent",
		"play_60_percent",
		"play_80_percent",
		"play_100_percent"
	};

	public override void _Ready()
	{
		FocusEntered += _OnFocusEntered;
		FocusExited += _OnFocusExited;
		ValueChanged += _OnValueChanged;

		// Configuration varies by the slider type.
		// Should be exposed as an enum when needed.
		MinValue = 20;
		MaxValue = 100;
		Step = 20;
	}

	public void _OnFocusEntered()
	{
		Voiceover.GlobalVoice.Play(AnnounceEvent);
	}

	public void _OnFocusExited()
	{
		Voiceover.GlobalVoice.Stop();
	}

	public void _OnValueChanged(double newValue)
	{
		int index = (int)Mathf.Floor((float)newValue / 20.0f) - 1;
		string ev = percentEvents[index];
		Debug.Log(Class.UserInterface, String.Format("Playing slider event {0}", ev));
		Voiceover.GlobalVoice.Play(percentEvents[index]);
	}
}
