using Godot;
using System;
using System.Runtime.ExceptionServices;

public partial class UserInterface : Node
{
	public static UserInterface Instance;

	[Export] AudioStreamPlayer VoiceOverPlayer;
	[Export(PropertyHint.Dir)] string VoiceOverDirectory;
	[Export] string StartMenu;
	Control ActiveMenu;
	bool HasGrabbedFirstFocus = false;
	string SecondInQueueName;

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		SetActiveMenu("");
		VoiceOverPlayer.Finished += _OnVoiceOverPlayerFinished;
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("toggle_menu"))
		{
			if (IsMenuActive()) SetActiveMenu("");
			else
			{
				SetActiveMenu(StartMenu);
				PlayVoiceOver("pausing");
			}
		}
		if ((Input.IsActionJustPressed("ui_down") || Input.IsActionJustPressed("ui_right")) && IsMenuActive())
		{
			GrabFirstFocus();
		}
	}

	public void _OnQuitGamePressed()
	{
		GetTree().ReloadCurrentScene();
	}

	void _OnVoiceOverPlayerFinished()
	{
		if (SecondInQueueName != "")
		{
			PlayVoiceOver(SecondInQueueName, "");
		}
	}

	void GrabFirstFocus()
	{
		if (HasGrabbedFirstFocus) return;
		HasGrabbedFirstFocus = true;

		Control first = ActiveMenu.FindChild("#*") as Control;
		if (first == null) Debug.Log(Debug.That.Interface, "Could not find valid focus child");
		else
		{
			Debug.Log(Debug.That.Interface, String.Format("Grabbig focus for {0}", first));
			first.CallDeferred(Control.MethodName.GrabFocus);
		}
	}

	public void SetActiveMenu(string menuName)
	{
		Node root = menuName == "" ? null : GetNode(menuName);
		foreach (Node child in GetChildren())
		{
			if (child is Control control && child.Name.ToString().StartsWith("Menu")) control.Hide();
		}

		Control PrevMenu = ActiveMenu;
		ActiveMenu = root as Control;
		if (ActiveMenu != null)
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
			ActiveMenu.CallDeferred(Control.MethodName.Show);
			HasGrabbedFirstFocus = false;
			if (PrevMenu != null) GrabFirstFocus();
		}
		else
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
			if (PrevMenu != null) PlayVoiceOver("resuming");
		}
	}

	public void PlayVoiceOver(string name1, string name2)
	{
		SecondInQueueName = name2;
		string path = String.Format("{0}/{1}.wav", VoiceOverDirectory, name1);
		AudioStream stream = GD.Load<AudioStream>(path);

		if (stream == null) Debug.Log(Debug.That.Interface, String.Format("Failed to load voice-over \"{0}\"", path));
		else Debug.Log(Debug.That.Interface, String.Format("Playing voice-over \"{0}\", stream {1}", path, stream));

		VoiceOverPlayer.Stream = stream;
		VoiceOverPlayer.Play();
	}

	public void PlayVoiceOver(string name)
	{
		PlayVoiceOver(name, "");
	}

	// For sliders to link here, will play a percentage sound of whatever is available.
	// post is a recording played after the value.
	public void PlayPercentage(float value, string post)
	{
		int index20 = Mathf.FloorToInt(value / 20.0f) - 1;
		string[] streamNames = {
			"20_percent",
			"40_percent",
			"60_percent",
			"80_percent",
			"100_percent"
		};
		PlayVoiceOver(streamNames[index20], post);
	}

	public bool IsMenuActive()
	{
		return ActiveMenu != null;
	}
}
