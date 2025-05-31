using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class NavDebugAgent : Node3D
{
	[Export]
	Array<Rid> Maps;

	[Export]
	int MapIndex = 0;

	enum State
	{
		Idle,
		Navigating
	}
	State state = State.Idle;

	ForestRoute Route;
	MeshInstance3D RouteMesh = new();
	Label3D Label;
	Label DebugLabel;

	Node3D StartPoint;
	Node3D EndPoint;

	public override void _Ready()
	{
		RouteMesh.TopLevel = true;
		AddChild(RouteMesh);
		RouteMesh.MaterialOverride = GD.Load<Material>("uid://34hcr5xyjpxv");

		Label = GetNode<Label3D>("%Label3D");
		DebugLabel = GetNode<Label>("%DebugLabel");

		StartPoint = GetNode<Node3D>("StartPoint");
		EndPoint = GetNode<Node3D>("EndPoint");

		CallDeferred(MethodName._OnNavigationReady);
	}

	void _OnNavigationReady()
	{
		Maps = NavigationServer3D.GetMaps();
	}

	public override void _Process(double delta)
	{
		switch (state)
		{
			case State.Idle:
				break;

			case State.Navigating:
				break;
		}
	}

	Vector3 GetCameraRayPosition()
	{
		Viewport vp = GetViewport();
		Camera3D cam = vp.GetCamera3D();
		Vector2 screenPoint = vp.GetMousePosition();

		PhysicsRayQueryParameters3D query = new();
		query.From = cam.ProjectRayOrigin(screenPoint);
		query.To = query.From + cam.ProjectRayNormal(screenPoint) * 9999.0f;

		var space = GetWorld3D().DirectSpaceState;
		Dictionary result = space.IntersectRay(query);

		Vector3 dest = Vector3.Zero;
		if (result.Count > 0)
		{
			dest = result["position"].AsVector3();
		}

		return dest;
	}

	public override void _Input(InputEvent ev)
	{
		if (ev.IsActionPressed("nav_debug_world_route"))
		{
			Route = ForestNetwork.Instance.GetClosestPathRoute(StartPoint.GlobalPosition, EndPoint.GlobalPosition);
			UpdateRouteMesh();
		}

		if (ev.IsActionPressed("nav_debug_junction_route"))
		{
			Route = ForestNetwork.Instance.GetClosestJunctionRoute(StartPoint.GlobalPosition, EndPoint.GlobalPosition);
			UpdateRouteMesh();
		}

		if (ev.IsActionPressed("nav_debug_set_end"))
		{
			EndPoint.GlobalPosition = GetCameraRayPosition();
		}

		if (ev.IsActionPressed("nav_debug_set_start"))
		{
			StartPoint.GlobalPosition = GetCameraRayPosition();
			UpdateDebugLabel();
		}
	}

	public void UpdateDebugLabel()
	{
		(float pathDist, GuidePath path) = ForestNetwork.Instance.GetClosestPath(StartPoint.GlobalPosition);
		(float junctionDist, Junction junc) = ForestNetwork.Instance.GetClosestJunction(StartPoint.GlobalPosition);

		string text = "";
		text += String.Format("dist={0:0.00}, path={1}\n", pathDist, path.Name);
		text += String.Format("dist={0:0.00}, junction={1}\n", junctionDist, junc.Name);

		DebugLabel.Text = text;
	}

	public void UpdateRouteMesh()
	{
		SurfaceTool st = new();
		st.Begin(Mesh.PrimitiveType.LineStrip);
		Color[] colors = [Colors.Green, Colors.Blue];

		Vector3[] positions = Route.GetPositions();
		for (int i = 0; i < positions.Length; i++)
		{
			Vector3 point = positions[i] + Vector3.Up * 0.1f;
			Color color = colors[i % 2];
			st.SetColor(color);
			st.AddVertex(point);
		}

		Label.Text = positions.Length.ToString();
		RouteMesh.Mesh = st.Commit();
	}
}
