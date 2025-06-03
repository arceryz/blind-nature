using System;
using System.Runtime.CompilerServices;
using Godot;
using Godot.Collections;

public partial class Debug
{
	public enum That
	{
		ForestNetwork,
		Kestrel,
		Vibration,
		Liora,
		LioraBow,
		Interface
	};
	public static Dictionary<That, bool> Settings = new();
	static bool IsReady = false;

	static void LoadSettings()
	{
		Settings[That.ForestNetwork] = ProjectSettings.GetSetting("debug/flags/forest_network").AsBool();
		Settings[That.Kestrel] = ProjectSettings.GetSetting("debug/flags/kestrel").AsBool();
		Settings[That.Vibration] = ProjectSettings.GetSetting("debug/flags/vibration").AsBool();
		Settings[That.Liora] = ProjectSettings.GetSetting("debug/flags/liora").AsBool();
		Settings[That.LioraBow] = ProjectSettings.GetSetting("debug/flags/liora_bow").AsBool();
		Settings[That.Interface] = ProjectSettings.GetSetting("debug/flags/interface").AsBool();
		IsReady = true;
	}

	public static void Log(That @class, string text)
	{
		if (!IsReady) LoadSettings();
		if (Settings[@class])
		{
			GD.Print(String.Format("{0,-15} {1}", @class.ToString() + ":", text));
		}
	}
}