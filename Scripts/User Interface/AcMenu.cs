using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

public partial class AcMenu : VBoxContainer
{
	[Export] Control FirstControl;

	public void GrabFirstFocus()
	{
		GetFirst().CallDeferred(Control.MethodName.GrabFocus);
		Debug.Log(Class.UserInterface, String.Format("Grabbig focus for {0}", GetFirst()));
	}

	public Control GetFirst() => FirstControl;
	public Control GetLast() => GetChild<Control>(GetChildCount() - 1);
}
