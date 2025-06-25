using Godot;

[GlobalClass]
public partial class HapticsProfile : Resource
{
	[ExportGroup("Directional Feedback")]
	[Export(PropertyHint.Range, "0, 1")] public float DfStrength;
	[Export(PropertyHint.Range, "0, 1")] public float DfIntervalFastest;
	[Export(PropertyHint.Range, "0, 1")] public float DfIntervalSlowest;
	[Export(PropertyHint.Range, "0, 1")] public float DfPulseDuration;

	[ExportGroup("Steps")]
	[Export] public Vector3 StepPulse;

	[ExportGroup("Rotation Feedback")]
	[Export] public Vector3 RfPulse90;
	[Export] public Vector3 RfPulse30;
	[Export] public float RfResetTime = 0.5f;
}
