using Godot;
using System;

public partial class AcButton : Button
{
	[Export] public string VoiceEvent;
	int eventId;

	public override void _Ready()
	{
		FocusEntered += _OnFocusEntered;
		Pressed += _OnButtonPressed;
	}

	public void _OnFocusEntered()
	{
		Voiceover.GlobalVoice.Play(VoiceEvent);
	}

	public void _OnButtonPressed()
	{
		Voiceover.GlobalVoice.Play(VoiceEvent);
	}
}
