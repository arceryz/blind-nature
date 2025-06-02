using Godot;

[GlobalClass]
public partial class VibrationProfile : Resource
{
	[ExportGroup("Directional Feedback")]
	[Export(PropertyHint.Range, "0, 1")] public float DfStrength;
	[Export(PropertyHint.Range, "0, 1")] public float DfIntervalFastest;
	[Export(PropertyHint.Range, "0, 1")] public float DfIntervalSlowest;
	[Export(PropertyHint.Range, "0, 1")] public float DfPulseDuration;

	[ExportGroup("Steps")]
	[Export(PropertyHint.Range, "0, 1")] public float StepStrong;
	[Export(PropertyHint.Range, "0, 1")] public float StepWeak;
	[Export(PropertyHint.Range, "0, 1")] public float StepPulseDuration;
}