using Godot;
using Godot.Collections;
using System;
using System.Data.Common;

// The globally accessible class for managing vibration.
// PS4 controllers require DS4Windows currently.
[GlobalClass]
public partial class Vibration : Node
{
	public static Vibration Instance;
	[Export] VibrationProfile Profile;
	int Device = 0;

	float PulseTimer = 0.0f;
	float PulseInterval = 0.0f;
	float PulseDuration = 0.0f;
	float PulseWeak = 0.0f;
	float PulseStrong = 0.0f;
	float PulseLock = 0.0f;

	public override void _EnterTree()
	{
		Instance = this;
		Profile = GD.Load<VibrationProfile>("res://settings/vibration_profile.tres");
	}

	public override void _Ready()
	{
		FindDevice();
	}

	public override void _Process(double delta)
	{
		// Play pulses.
		if (PulseInterval > 0.0f)
		{
			if (PulseTimer > PulseInterval && PulseLock == 0.0f)
			{
				PulseTimer = 0.0f;
				PulseInterval = 0.0f;
				PlayRaw(PulseWeak, PulseStrong, PulseDuration);
			}
			PulseTimer += (float)delta;
			PulseLock = Mathf.Max(0.0f, PulseLock - (float)delta);
		}
	}

	void FindDevice()
	{
		Array<int> joys = Input.GetConnectedJoypads();
		Debug.Log(Debug.That.Vibration, "Listing connected controllers:");

		int selectedId = -1;
		foreach (int id in joys)
		{
			string name = Input.GetJoyName(id);
			string guid = Input.GetJoyGuid(id);
			if (guid.Contains("XINPUT") || name.Contains("XBOX"))
			{
				selectedId = id;
			}
			Debug.Log(Debug.That.Vibration, String.Format("Controller {0}: {1}, {2}", id, name, guid));
		}

		if (selectedId == -1)
		{
			Debug.Log(Debug.That.Vibration, "No suitable XINPUT controller detected, using default");
		}
		else
		{
			Debug.Log(Debug.That.Vibration, "Using controller " + selectedId);
		}
	}

	void PlayRaw(float weak, float strong, float duration)
	{
		Input.StartJoyVibration(Device, weak, strong, duration);
	}

	/// <summary>
	/// Play vibration briefly with intensity based on the direction to the target.
	/// The target is relative to the origin of the aim in the zero vector.
	/// The radius is used in calculating whether the current direction is on a collision course.
	/// </summary>
	public void PlayDirectionalFeedback(Vector2 direction, Vector2 target, float targetRadius)
	{
		Vector2 opt = target.Normalized();
		Vector2 opt_left = (new Vector2(-opt.Y, opt.X) * targetRadius + target).Normalized();
		float alpha = opt.Dot(opt_left);
		float alignment = direction.IsZeroApprox() ? 0.0f : Mathf.Min(1.0f, (opt.Dot(direction) + 1.0f) / (alpha + 1.0f));
		if (target.Length() < targetRadius) alignment = 1.0f;

		PulseDuration = Profile.DfPulseDuration;
		PulseStrong = Profile.DfStrength;
		PulseWeak = Profile.DfStrength;
		PulseInterval = Mathf.Lerp(Profile.DfIntervalSlowest, Profile.DfIntervalFastest, alignment);

		Debug.Log(Debug.That.Vibration, String.Format("dir={0}, target={1}, r={2}, align={3}, inter={4}", direction, target, targetRadius, alignment, PulseInterval));
	}

	/// <summary>
	/// Play a vibration for a footstep once. Should be called at each step.
	/// Locks any pulses from overriding this vibration.
	/// </summary>
	/// <param name="strength"> The strength of this step from 0 to 1. </param>
	public void PlayStep(float strength)
	{
		float weak = Profile.StepWeak;
		float strong = Profile.StepStrong;
		PulseLock = Profile.StepPulseDuration;
		PlayRaw(weak, strong, Profile.StepPulseDuration);
	}
}
