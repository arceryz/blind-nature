using Godot;

[GlobalClass]
public partial class VibrationProfile : Resource
{
	[ExportGroup("Directional Feedback")]
	[Export(PropertyHint.Range, "0, 1")] public float df_strength;
	[Export(PropertyHint.Range, "0, 1")] public float df_interval_fastest;
	[Export(PropertyHint.Range, "0, 1")] public float df_interval_slowest;
	[Export(PropertyHint.Range, "0, 1")] public float df_pulse_duration;

	[ExportGroup("Steps")]
	[Export(PropertyHint.Range, "0, 1")] public float step_strength;
	[Export(PropertyHint.Range, "0, 1")] public float step_strong_balance;
	[Export(PropertyHint.Range, "0, 1")] public float step_pulse_duration;
}