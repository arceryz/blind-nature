using Godot;
using Godot.Collections;
using System;
using System.Data.Common;

// The globally accessible class for managing vibration.
// PS4 controllers require DS4Windows currently.
public partial class Vibration : Node
{
	public static Vibration Instance;
	VibrationProfile Profile;
	int Device = 0;

	float PulseTimer = 0.0f;
	float PulseInterval = 0.0f;
	float PulseDuration = 0.0f;
	float PulseWeak = 0.0f;
	float PulseStrong = 0.0f;

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
			if (PulseTimer > PulseInterval)
			{
				PulseTimer = 0.0f;
				PlayRaw(PulseWeak, PulseStrong, PulseDuration);
			}
			PulseTimer += (float)delta;
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
	/// The radius is used in calculating whether the current direction is on a collision course.
	/// </summary>
	public void PlayDirectionalFeedback(Vector2 origin, Vector2 direction, Vector2 target, float targetRadius)
	{
		Vector2 opt = (target - origin).Normalized();
		Vector2 opt_left = (new Vector2(-opt.Y, opt.X) * targetRadius + target - origin).Normalized();
		float alpha = opt.Dot(opt_left);
		float alignment = Mathf.Min(1.0f, (opt.Dot(direction) + 1.0f) / (alpha + 1.0f));

		PulseDuration = Profile.df_pulse_duration;
		PulseStrong = Profile.df_strength;
		PulseWeak = Profile.df_strength;
		PulseInterval = Mathf.Lerp(Profile.df_interval_slowest, Profile.df_interval_fastest, alignment);
	}

	/// <summary>
	/// Play a vibration for a footstep once. Should be called at each step.
	/// </summary>
	/// <param name="strength"> The strength of this step from 0 to 1. </param>
	public void PlayStep(float strength)
	{
		float weak = Profile.step_strength * (1.0f - Profile.step_strong_balance);
		float strong = Profile.step_strength * Profile.step_strong_balance;
		PlayRaw(weak, strong, Profile.step_pulse_duration);
	}
}
