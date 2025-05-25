using System;
using Godot;

[GlobalClass]
public partial class ForestNetwork : Node
{
	Node JunctionList;
	Node PathList;

	AStar3D graph = new();

	public override void _Ready()
	{
		JunctionList = GetNode<Node>("Junctions");
		PathList = GetNode<Node>("Paths");

		for (int id = 0; id < JunctionList.GetChildCount(); id++)
		{
			Node3D node = JunctionList.GetChild<Node3D>(id);
			graph.AddPoint(id, node.GlobalPosition, 1.0f); // Weight scale can be used to put lower preference on a point!
			Debug.Instance.Log(Debug.Class.ForestNetwork, String.Format("Adding node {0} with id={1} and index={2}", node.ToString(), id, node.GetIndex()));
		}

		foreach (GuidePath path in PathList.GetChildren())
		{
			graph.ConnectPoints(path.Junction1.GetIndex(), path.Junction2.GetIndex());
		}
	}
}