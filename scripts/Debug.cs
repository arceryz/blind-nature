using Godot;
using Godot.Collections;

public partial class Debug: Node
{
	public static Debug Instance;

	public enum Class
	{
		ForestNetwork,
		Kestrel,
	};

	public Dictionary<Class, bool> Settings = new();

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		Settings[Class.ForestNetwork] = ProjectSettings.GetSetting("debug/flags/forest_network").AsBool();
		Settings[Class.Kestrel] = ProjectSettings.GetSetting("debug/flags/kestrel").AsBool();
	}

	public void Log(Class @class, string text)
	{
		if (Settings[@class])
		{
			GD.Print("LOG: " + text);
		}
	}
}