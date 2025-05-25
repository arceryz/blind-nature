using Godot;
using Godot.Collections;

public partial class Debug: Node
{
	public static Debug Instance;

	public enum Class
	{
		ForestNetwork,
	};

	public Dictionary<Class, bool> Settings = new();

	public override void _Ready()
	{
		Instance = this;
		Settings[Class.ForestNetwork] = ProjectSettings.GetSetting("debug/flags/forest_network").AsBool();
	}

	public void Log(Class @class, string text)
	{
		if (Settings[@class])
		{
			GD.Print("LOG: " + text);
		}
	}
}