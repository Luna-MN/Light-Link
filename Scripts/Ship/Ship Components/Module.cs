using Godot;
using System;
using System.Collections.Generic;
[GlobalClass]
public partial class Module : Node2D
{
	public bool placed;
	public Node2D meshParent;
	public ModuleUI.ModuleName moduleName;
	private Camera cam;
	private PlayerCreatedShip ship;
	private List<AttachmentPoint> PossiblePoints = new List<AttachmentPoint>();
	private AttachmentPoint closestPoint;
	private Line2D AttachmentLine;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Modulate = new Color(1, 1, 1, 0.5f);
		
		cam = ((Camera)GetViewport().GetCamera2D());
		if (cam.ships.Count == 1 && cam.ships[0] is PlayerCreatedShip shipL)
		{
			ship = shipL;
			ship.ShowNodes(true);
			cam.ModulePlacing = true;
			foreach (var ap in ship.shipNodes)
			{
				if (moduleName.ToString() == ap.NodeType.ToString())
				{
					PossiblePoints.Add(ap);
				}
				else if (moduleName.ToString() == ap.NodeType.ToString())
				{
					PossiblePoints.Add(ap);
				}
			}
		}
		AttachmentLine = new Line2D();
		AttachmentLine.Width = 0.5f;
		AddChild(AttachmentLine);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!placed && PossiblePoints.Count > 0)
		{
			GlobalPosition = GetGlobalMousePosition();
			closestPoint = PossiblePoints[0];
			foreach (var ap in PossiblePoints)
			{
				if (ap.GlobalPosition.DistanceTo(GlobalPosition) < closestPoint.GlobalPosition.DistanceTo(GlobalPosition))
				{
					closestPoint = ap;
				}
			}
			AttachmentLine.ClearPoints();
			if (closestPoint.GlobalPosition.DistanceTo(GlobalPosition) < 100)
			{
				AttachmentLine.AddPoint(ToLocal(GlobalPosition));
				AttachmentLine.AddPoint(ToLocal(closestPoint.GlobalPosition));
			}
		}
		else if (PossiblePoints.Count == 0)
		{
			QueueFree();
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButtonEvent)
		{
			if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.Pressed && !placed)
			{
				if (closestPoint != null && closestPoint.GlobalPosition.DistanceTo(GlobalPosition) < 100)
				{
					placed = true;
					Modulate = new Color(1, 1, 1, 1);
					Position = closestPoint.Position;
					AttachmentLine?.QueueFree();
					cam.suppressMovmentTimer.Start();
					ship.ShowNodes(false);
					Placed(ship);
					ship.shipNodes.Remove(closestPoint);
				}
			}
		}
	}

	public virtual void Placed(PlayerCreatedShip ship)
	{
		ship.modules.Add(this);
	}
}
	