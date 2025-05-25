using Godot;
using System;
using System.Threading.Tasks;

public partial class Kestrel : Node3D
{
	Junction Destination;

	enum State
	{
		Positioning,
		Calling,
		Idle
	}
	State state = State.Idle;

	public override void _Ready()
	{
		// Fetch route once the network is ready.
		if (!ForestNetwork.Instance.IsNodeReady())
			ForestNetwork.Instance.Ready += _NetworkReady;
		else
			_NetworkReady();
	}

	public void _NetworkReady()
	{

	}

	public override void _Process(double delta)
	{
		switch (state)
		{
			case State.Positioning:
				break;

			case State.Calling:
				break;

			case State.Idle:
				break;
		}
	}

	public void NavigateTo(Junction dest)
	{
		Destination = dest;
		
	}
}
