using Godot;
using System;

[Tool]
public partial class Junction : Node3D
{
	Label3D label;

	public override void _Ready()
	{
		label = GetNode<Label3D>("Label3D");
		SetLabel();
	}

	void SetLabel()
	{
		if (label == null)
			return;
		label.Text = Name;
	}
}
