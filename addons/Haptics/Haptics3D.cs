using Godot;
using System;

[GlobalClass, Icon("res://addons/Haptics/Haptics3D.svg")]
public partial class Haptics3D : Marker3D, IHaptics
{
	public void PlayDirectionalFeedback(Vector2 d, Vector2 t, float r) => Haptics.Instance.PlayDirectionalFeedback(d, t, r);
	public void PlayStep(float s) => Haptics.Instance.PlayStep(s);
	public void PlayRotationFeedback(float delta) => Haptics.Instance.PlayRotationFeedback(delta);
}
