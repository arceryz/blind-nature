using Godot;
using Godot.Collections;
using System;
using System.Data.Common;
using System.Security.Cryptography;

// The globally accessible class for managing vibration.
// PS4 controllers require DS4Windows currently.

public partial class Haptics : Node
{
	public static Haptics Instance;
	public HapticsProfile Profile;
	int Device = 0;

	float PulseTimer = 0.0f;
	float PulseInterval = 0.0f;
	float PulseDuration = 0.0f;
	float PulseWeak = 0.0f;
	float PulseStrong = 0.0f;
	float PulseLock = 0.0f;

	float RotationSign = 1.0f;
	int RotationAccumulator = 0;
	float RotationTickAccumulator = 0.0f;
	float RotationSpeed = 0.0f;
	public QuickTimer RotationFeedbackResetTimer;

	public override void _EnterTree()
	{
		Instance = this;
		Profile = GD.Load<HapticsProfile>("Scripts/Haptics/haptics_profile.tres");
	}

	public override void _Ready()
	{
		FindDevice();
		RotationFeedbackResetTimer = new QuickTimer(this, () =>
		{
			RotationAccumulator = 0;
			RotationTickAccumulator = 0.0f;
			Debug.Log(Class.Haptics, String.Format("Rotation feedback: sign={0}, acc={1}, tick={2}", RotationSign, RotationAccumulator, RotationTickAccumulator));
		}, Profile.RfResetTime);
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
		Debug.Log(Class.Haptics, "Listing connected controllers:");

		int selectedId = -1;
		foreach (int id in joys)
		{
			string name = Input.GetJoyName(id);
			string guid = Input.GetJoyGuid(id);
			if (guid.Contains("XINPUT") || name.Contains("XBOX"))
			{
				selectedId = id;
			}
			Debug.Log(Class.Haptics, String.Format("Controller {0}: {1}, {2}", id, name, guid));
		}

		if (selectedId == -1)
		{
			Debug.Log(Class.Haptics, "No suitable XINPUT controller detected, using default");
		}
		else
		{
			Debug.Log(Class.Haptics, "Using controller " + selectedId);
		}
	}

	public void PlayRaw(float weak, float strong, float duration)
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

		Debug.Log(Class.Haptics, String.Format("dir={0}, target={1}, r={2}, align={3}, inter={4}", direction, target, targetRadius, alignment, PulseInterval));
	}

	/// <summary>
	/// Play a vibration for a footstep once. Should be called at each step.
	/// Locks any pulses from overriding this vibration.
	/// </summary>
	/// <param name="strength"> The strength of this step from 0 to 1. </param>
	public void PlayStep(float strength)
	{
		float weak = Profile.StepPulse.X;
		float strong = Profile.StepPulse.Y;
		PulseLock = Profile.StepPulse.Z;
		PlayRaw(weak, strong, Profile.StepPulse.Z);
	}

	public void PlayRotationFeedback(float delta)
	{
		float newSign = Mathf.Sign(delta);
		if (newSign != 0)
		{
			if (newSign != RotationSign)
			{
				// Reset the accumulator if the sign has changed.
				RotationAccumulator = 0;
				RotationTickAccumulator = 0.0f;
			}
			RotationSign = newSign;
			RotationFeedbackResetTimer.Start();
			RotationSpeed = Mathf.Abs(delta) / (float)GetPhysicsProcessDeltaTime();
		}

		RotationTickAccumulator += Mathf.Abs(delta);

		if (RotationTickAccumulator > 30.0f)
		{
			RotationTickAccumulator -= 30.0f;
			RotationAccumulator += 30;

			Vector3 pulse = Profile.RfPulse30;
			if (RotationAccumulator % 90 == 0) pulse = Profile.RfPulse90;
			if (pulse.Z > 0) PlayRaw(pulse.X, pulse.Y, pulse.Z);
		}

		Debug.Log(Class.Haptics, String.Format("Rotation feedback: sign={0}, acc={1}, tick={2}, speed={3}", RotationSign, RotationAccumulator, RotationTickAccumulator, RotationSpeed));
	}
}
