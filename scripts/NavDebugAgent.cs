using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class NavDebugAgent : Node3D
{
	public static NavDebugAgent Instance;

	[Export]
	Array<Rid> Maps;

	[Export]
	int MapIndex = 0;
	
	ForestRoute Route;
	MeshInstance3D RouteMesh = new();
	Label3D Label;
	Label DebugLabel;

	Node3D StartPoint;
	Node3D EndPoint;
	Node3D PathPoint;

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		RouteMesh.TopLevel = true;
		AddChild(RouteMesh);
		RouteMesh.MaterialOverride = GD.Load<Material>("uid://34hcr5xyjpxv");

		Label = GetNode<Label3D>("%Label3D");
		DebugLabel = GetNode<Label>("%DebugLabel");

		StartPoint = GetNode<Node3D>("StartPoint");
		EndPoint = GetNode<Node3D>("EndPoint");
		PathPoint = GetNode<Node3D>("PathPoint");
	}

	public override void _Process(double delta)
	{
		UpdateDebugLabel();
		if (Input.IsActionPressed("nav_debug_set_end"))
		{
			EndPoint.GlobalPosition = GetCameraRayPosition();
		}

		if (Input.IsActionPressed("nav_debug_set_start"))
		{
			StartPoint.GlobalPosition = GetCameraRayPosition();
			UpdateDebugLabel();
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
	}

	public void UpdateDebugLabel()
	{
		(float pathDist, GuidePath path) = ForestNetwork.Instance.GetClosestPath(StartPoint.GlobalPosition);
		(float junctionDist, Junction junc) = ForestNetwork.Instance.GetClosestJunction(StartPoint.GlobalPosition);
		float totalOffset = 0.0f;
		Vector3 offsetPoint = Vector3.Zero;
		if (Route != null)
		{
			totalOffset = Route.GetClosestOffset(StartPoint.GlobalPosition);
			offsetPoint = Route.Sample(totalOffset);
		}

		string text = "";
		text += String.Format("dist={0:0.00}, path={1}\n", pathDist, path.Name);
		text += String.Format("dist={0:0.00}, junction={1}\n", junctionDist, junc.Name);
		text += String.Format("totalOffset={0:0.00}\n", totalOffset);
		text += String.Format("stray={0:0.00}\n", (StartPoint.GlobalPosition-offsetPoint).Length());
		text += String.Format("stepAccum={0:0.0}\n", Liora.Instance.StepAccumulator);

		PathPoint.GlobalPosition = offsetPoint;
		DebugLabel.Text = text;
	}

	public void SetRoute(ForestRoute route)
	{
		StartPoint.GlobalPosition = route.Junctions[0].GlobalPosition;
		EndPoint.GlobalPosition = route.Junctions.Last().GlobalPosition;
		Route = route;
		UpdateRouteMesh();
		UpdateDebugLabel();
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
