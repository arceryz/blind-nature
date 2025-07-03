using Godot;
using System;
using System.Data.Common;

public partial class WwiseSharp : Node
{
	public static WwiseSharp Instance;
	public static void AkEventPost(Node akEvent) => akEvent.Call("post_event");
	public static void AkEventStop(Node akEvent) => akEvent.Call("stop_event");

	GodotObject Translator;
	GodotObject Wwise;

	public override void _Ready()
	{
		Instance = this;
		Translator = (GodotObject)GD.Load<GDScript>("uid://bgtu4ebxh067j").New();
		Wwise = Translator.Call("get_wwise_singleton").AsGodotObject();

		Debug.Log(Class.Wwise, "Wwise translator singleton: " + Translator);
		Debug.Log(Class.Wwise, "Wwise singleton: " + Wwise);
	}

	public int PostEvent(string eventName, Node gameObject) => Wwise.Call("post_event", eventName, gameObject).AsInt32();
	public void StopEvent(int id, int fadeTime=0, int interpolation=0) => Wwise.Call("stop_event", id, fadeTime, interpolation);
	/// <summary>
	/// Set an RTPC value in Wwise. If gameObject is null, then the value
	/// is set globally for all objects.
	/// </summary>
	public void SetRtpc(string  rtpcName, float value, Node gameObject = null) => Wwise.Call("set_rtpc_value", rtpcName, value, gameObject);
	public void SetSwitch(string switchGroup, string value, Node gameObject) => Wwise.Call("set_switch", switchGroup, value, gameObject);
}
