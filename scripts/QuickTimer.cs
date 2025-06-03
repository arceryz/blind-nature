using System;
using System.Runtime.CompilerServices;
using Godot;

public partial class QuickTimer : Node
{
	public event Action Timeout;

	public float BaseDuration = 0.0f;
	public float RandomDuration = 0.0f;
	bool Active = false;
	bool Oneshot = false;
	bool CallDirect = false;
	float Time = 0.0f;
	float Duration = 0.0f;

	public QuickTimer(Node bind, Action callback, float baseDur, float randomDur, bool oneshot, bool callDirect)
	{
		bind.AddChild(this);
		Timeout += callback;
		BaseDuration = baseDur;
		RandomDuration = randomDur;
		Oneshot = oneshot;
		CallDirect = callDirect;

	}

	public override void _Process(double delta)
	{
		if (Active) Time += (float)delta;
		if (Time > Duration)
		{
			Timeout?.Invoke();
			Reset();
			if (Oneshot) Active = false;
		}
	}

	void Reset()
	{
		Time = 0.0f;
		Duration = BaseDuration + GD.Randf() * RandomDuration;
	}

	public void Start()
	{
		Active = true;
		Reset();
		if (CallDirect) Timeout?.Invoke();
	}

	public void Stop()
	{
		Active = false;
	}

	public bool IsFinished()
	{
		return !Active;
	}
}