using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

public partial class UserInterface : CanvasLayer
{
	public static UserInterface Instance;

	Button BackButton;
	AcMenu activeMenu;
	AcMenu startMenu;
	List<AcMenu> menus = new();
	bool hasGrabbedFirstFocus = false;

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		foreach (Node child in GetChildren())
		{
			if (child is AcMenu menu)
			{
				menus.Add(menu);
				Debug.Log(Class.UserInterface, String.Format("Adding menu {0}", menu));
			}
		}
		startMenu = menus[0];

		BackButton = GetNode<Button>("%BackButton");
		BackButton.Pressed += _OnBackButtonPressed;
		
		SetActiveMenu(null);
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("toggle_menu"))
		{
			if (IsMenuActive()) SetActiveMenu(null);
			else SetActiveMenu(startMenu);
		}
		if ((Input.IsActionJustPressed("ui_down") || Input.IsActionJustPressed("ui_right")) && IsMenuActive() && !hasGrabbedFirstFocus)
		{
			activeMenu.GrabFirstFocus();
			hasGrabbedFirstFocus = true;
		}
	}

	public void SetActiveMenu(AcMenu menu)
	{
		foreach (AcMenu m in menus)
		{
			m.Hide();
		}

		if (menu != null)
		{
			if (activeMenu == null) Voiceover.GlobalVoice.Play("play_pausing");
			Input.MouseMode = Input.MouseModeEnum.Visible;
			hasGrabbedFirstFocus = false;
			menu.CallDeferred(Control.MethodName.Show);
			Show();

			if (menu == startMenu)
			{
				BackButton.Hide();
			}
			else
			{
				BackButton.Reparent(menu);
				BackButton.Show();
			}
		}
		else
		{
			if (activeMenu != null) Voiceover.GlobalVoice.Play("play_resuming");
			Input.MouseMode = Input.MouseModeEnum.Captured;
			Hide();
		}
		activeMenu = menu;
	}

	public bool IsMenuActive()
	{
		return activeMenu != null;
	}

	void _OnBackButtonPressed()
	{
		SetActiveMenu(startMenu);
	}
}
