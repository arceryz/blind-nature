using Godot;
using System;

public partial class GameUI : UserInterface
{
	AcMenu MenuMain;
	AcMenu MenuSettings;

	Button SettingsButton;
	Button ResumeButton;
	Button QuitButton;

	Slider VolumeSlider;

	public override void _Ready()
	{
		MenuMain = GetNode<AcMenu>("%MenuMain");
		MenuSettings = GetNode<AcMenu>("%MenuSettings");

		SettingsButton = GetNode<Button>("%SettingsButton");
		SettingsButton.Pressed += _OnSettingsButtonPressed;

		ResumeButton = GetNode<Button>("%ResumeButton");
		ResumeButton.Pressed += _OnResumeButtonPressed;

		QuitButton = GetNode<Button>("%QuitButton");
		QuitButton.Pressed += _OnQuitButtonPressed;

		VolumeSlider = GetNode<Slider>("%VolumeSlider");
		VolumeSlider.ValueChanged += _OnVolumeValueChanged;

		base._Ready();
	}

	void _OnSettingsButtonPressed()
	{
		SetActiveMenu(MenuSettings);
	}

	void _OnResumeButtonPressed()
	{
		SetActiveMenu(null);
	}

	void _OnQuitButtonPressed()
	{
		GetTree().Quit();
	}

	void _OnVolumeValueChanged(double newValue)
	{
		WwiseSharp.Instance.SetRtpc("master_volume", (float)newValue);
	}
}
