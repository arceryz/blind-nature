using Godot;
using System;

[Tool]
[GlobalClass]
public partial class Junction : Area3D
{
	[Signal]
	public delegate void PlayerEnteredEventHandler();

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

	void _OnAreaBodyEntered(Node3D body)
	{
		if (body is Liora)
		{
			EmitSignal(SignalName.PlayerEntered);
		}
	}
}
