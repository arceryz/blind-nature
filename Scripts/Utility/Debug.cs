using System;
using Godot;

public enum Class
{
	ForestNetwork,
	Kestrel,
	Haptics,
	Liora,
	LioraBow,
	UserInterface,
	Wwise,
	SurfaceDetector,
};

public static class Debug
{

	public static void Log(Class cl, string text)
	{
		if (DebugSettings.Instance.IsClassEnabled(cl))
		{
			GD.Print(String.Format("{0,-15} {1}", cl.ToString() + ":", text));
		}
	}
}