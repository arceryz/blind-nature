using Godot;
using System;

public partial class Blindfold : TextureRect
{
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("toggle_blindfold"))
		{
			Visible = !Visible;
		}
	}
}
