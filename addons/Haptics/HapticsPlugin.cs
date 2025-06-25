#if TOOLS
using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class HapticsPlugin : EditorPlugin
{
	public static StringName ProfileSetting = "haptics/setup/profile";

	public void CreateNewSetting(string name, string value, bool isFile)
	{
		if (!ProjectSettings.HasSetting(name))
		{
			ProjectSettings.SetSetting(name, value);
		}
		if (isFile)
		{
			ProjectSettings.AddPropertyInfo(new Dictionary{
				{"name", name},
				{"type", (int)Variant.Type.String},
				{"hint", (int)PropertyHint.File},
			});
		}
	}

	public override void _EnterTree()
	{
		CreateNewSetting(ProfileSetting, "res://addons/Haptics/haptics_profile.tres", true);
		AddAutoloadSingleton("Haptics", "res://addons/Haptics/Haptics.cs");
	}

	public override void _ExitTree()
	{
		RemoveAutoloadSingleton("Haptics");
		ProjectSettings.SetSetting(ProfileSetting, new Variant());
	}
}
#endif
