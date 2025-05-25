using System;
using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;

public partial class NetworkAStar : AStar3D
{
	Dictionary<long, Dictionary<long, GuidePath>> Paths = new();

	public override float _ComputeCost(long fromId, long toId)
	{
		return Paths[fromId][toId].GetLength();
	}

	public void AddPath(long fromId, long toId, GuidePath path)
	{
		if (!Paths.ContainsKey(fromId))
		{
			Paths[fromId] = new();
		}
		if (!Paths.ContainsKey(toId))
		{
			Paths[toId] = new();
		}
		Paths[fromId][toId] = path;
		Paths[toId][fromId] = path;
		ConnectPoints(fromId, toId);
	}

	public GuidePath GetPath(long fromId, long toId)
	{
		return Paths[fromId][toId];
	}
}

[GlobalClass]
public partial class ForestNetwork : Node
{
	public static ForestNetwork Instance;

	Node JunctionList;
	Node PathList;

	NetworkAStar graph = new();

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		JunctionList = GetNode<Node>("Junctions");
		PathList = GetNode<Node>("Paths");

		for (int id = 0; id < JunctionList.GetChildCount(); id++)
		{
			Node3D node = JunctionList.GetChild<Node3D>(id);
			graph.AddPoint(id, node.GlobalPosition, 1.0f); // Weight scale can be used to put lower preference on a point!
			Debug.Instance.Log(Debug.Class.ForestNetwork, String.Format("Adding junction {0} with id={1} and index={2}", node.Name, id, node.GetIndex()));
		}

		foreach (GuidePath path in PathList.GetChildren())
		{
			long id1 = path.Junction1.GetIndex();
			long id2 = path.Junction2.GetIndex();

			graph.AddPath(id1, id2, path);
			Debug.Instance.Log(Debug.Class.ForestNetwork, String.Format("Adding path {0} with length {1:0.#}", path.Name, path.GetLength()));
		}
	}

	public Array<GuidePath> GetRoute(Junction from, Junction to)
	{
		Array<GuidePath> paths = new();

		long[] idPath = graph.GetIdPath(from.GetIndex(), to.GetIndex());
		for (int i = 0; i < idPath.Length - 1; i++)
		{
			GuidePath path = graph.GetPath(idPath[i], idPath[i + 1]);
			paths.Append(path);
			Debug.Instance.Log(Debug.Class.ForestNetwork, String.Format("Take path {0}", path.Name));
		}
		return paths;
	}
}