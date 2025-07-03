using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using Godot.Collections;

[GlobalClass]
[Tool]
public partial class DebugSettings : Node
{
	public static DebugSettings Instance;
	Dictionary<Class, bool> classEnabled = new();

	public override void _EnterTree()
	{
		Instance = this;
	}

	public bool IsClassEnabled(Class cl)
	{
		return classEnabled[cl];
	}

	public override Array<Dictionary> _GetPropertyList()
	{
		Array<Dictionary> properties = new();
		foreach (Class cl in Enum.GetValues<Class>())
		{
			properties.Add(new Dictionary{
				{ "name", Enum.GetName<Class>(cl) },
				{ "type", (int)Variant.Type.Bool },
			});
		}
		return properties;
	}

	public override Variant _Get(StringName property)
	{
		string str = property.ToString();
		if (Enum.TryParse<Class>(str, true, out Class cl))
		{
			if (!classEnabled.ContainsKey(cl))
			{
				classEnabled[cl] = false; // Default to false if not set
			}
			return classEnabled[Enum.Parse<Class>(property.ToString())];
		}
		return default;
	}

	public override bool _Set(StringName property, Variant value)
	{
		string str = property.ToString();
		if (Enum.TryParse<Class>(str, true, out Class cl))
		{
			classEnabled[cl] = value.As<bool>();
			return true;
		}
		return false;
	}
}