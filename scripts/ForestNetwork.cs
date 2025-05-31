using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Godot;
using System.IO;

public class ForestRoute
{
	public Junction[] Junctions;

	public ForestRoute(Junction[] _junctions)
	{
		Junctions = _junctions;
	}

	public float GetLength()
	{
		float sum = 0.0f;
		for(int i = 0; i < Junctions.Length-1; i++)
		{
			sum += ForestNetwork.Instance.GetGuidePath(Junctions[i], Junctions[i + 1]).GetLength();
		}
		return sum;
	}

	public Vector3[] GetPositions()
	{
		Vector3[] positions = new Vector3[Junctions.Length];
		for (int i = 0; i < Junctions.Length; i++)
		{
			positions[i] = Junctions[i].GlobalPosition;
		}
		return positions;
	}

	public override string ToString()
	{
		string s = String.Format("Route has {0} junctions\n", Junctions.Length);
		foreach (Junction j in Junctions)
		{
			s += j.Name + "\n";
		}
		return s;
	}
}

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
		if (Paths.ContainsKey(fromId))
		{
			if (Paths[fromId].ContainsKey(toId))
			{
				return Paths[fromId][toId];
			}
		}
		return null;
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

	public GuidePath GetGuidePath(Junction from, Junction to)
	{
		return graph.GetPath(from.GetIndex(), to.GetIndex());
	}

	public ForestRoute GetRoute(Junction from, Junction to)
	{
		long[] idPath = graph.GetIdPath(from.GetIndex(), to.GetIndex());
		Junction[] junctions = new Junction[idPath.Length];
		for (int i = 0; i < idPath.Length; i++)
		{
			junctions[i] = JunctionList.GetChild<Junction>((int)idPath[i]);
		}
		return new ForestRoute(junctions);
	}

	public (float, GuidePath) GetClosestPath(Vector3 from)
	{
		float distance = 0.0f;
		GuidePath closest = null;

		foreach (GuidePath path in PathList.GetChildren())
		{
			float d = path.GetDistance(from);
			if (d < distance || closest == null)
			{
				distance = d;
				closest = path;
			}
		}

		return (distance, closest);
	}

	public (float, Junction) GetClosestJunction(Vector3 from)
	{
		float distance = 0.0f;
		Junction closest = null;

		foreach (Junction junction in JunctionList.GetChildren())
		{
			float d = junction.GlobalPosition.DistanceTo(from);
			if (d < distance || closest == null)
			{
				distance = d;
				closest = junction;
			}
		}

		return (distance, closest);
	}

	public ForestRoute GetWorldRoute(Vector3 from, Vector3 to)
	{
		return new ForestRoute([]);
	}

	public ForestRoute GetClosestJunctionRoute(Vector3 from, Vector3 to)
	{
		(_, Junction j1) = ForestNetwork.Instance.GetClosestJunction(from);
		(_, Junction j2) = ForestNetwork.Instance.GetClosestJunction(to);

		return GetRoute(j1, j2);
	}

	public ForestRoute GetClosestPathRoute(Vector3 from, Vector3 to)
	{
		// Get the two closest paths.
		(_, GuidePath fromPath) = GetClosestPath(from);
		(_, GuidePath toPath) = GetClosestPath(to);

		// Get the two offsets from the start junction in each path.
		(_, float fromOffset) = fromPath.GetClosestPoint(from);
		(_, float toOffset) = toPath.GetClosestPoint(to);
		float fromOffsetC = fromPath.GetLength() - fromOffset;
		float toOffsetC = toPath.GetLength() - toOffset;

		// We will evaluate the four routes from each pair of junctions.
		ForestRoute r11 = GetRoute(fromPath.Junction1, toPath.Junction1);
		ForestRoute r12 = GetRoute(fromPath.Junction1, toPath.Junction2);
		ForestRoute r21 = GetRoute(fromPath.Junction2, toPath.Junction1);
		ForestRoute r22 = GetRoute(fromPath.Junction2, toPath.Junction2);

		// The route we end up taking is the one which has the shortest distance
		// when summing up the offsets+route length.
		float c11 = fromOffset + r11.GetLength() + toOffset;
		float c12 = fromOffset + r12.GetLength() + toOffsetC;
		float c21 = fromOffsetC + r21.GetLength() + toOffset;
		float c22 = fromOffsetC + r22.GetLength() + toOffsetC;

		float min = Mathf.Min(Mathf.Min(c11, c12), Mathf.Min(c21, c22));
		if (min == c11)
		{
			return new ForestRoute([fromPath.Junction2, .. r11.Junctions, toPath.Junction2]);
		}
		else if (min == c12)
		{
			return new ForestRoute([fromPath.Junction2, .. r12.Junctions, toPath.Junction1]);
		}
		else if (min == c21)
		{
			return new ForestRoute([fromPath.Junction1, .. r21.Junctions, toPath.Junction2]);
		}
		else if (min == c22)
		{
			return new ForestRoute([fromPath.Junction1, .. r22.Junctions, toPath.Junction1]);
		}
		else
		{
			GD.PushError("Minimum route has no match");
			return null;
		}
	}
}