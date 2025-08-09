using Godot;
using System;
using System.Collections.Generic;

public partial class Module : Node2D
{
	public bool placed;
	public Node2D meshParent;
	public ModuleUI.ModuleName moduleName;
	private Camera cam;
	private PlayerCreatedShip ship;
	private List<AttachmentPoint> PossiblePoints = new List<AttachmentPoint>();
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
			foreach (var ap in ship.attachmentPoints)
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
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!placed)
		{
			Position = GetGlobalMousePosition();
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButtonEvent)
		{
			if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.Pressed)
			{
				placed = true;
				Modulate = new Color(1, 1, 1, 1);
				cam.suppressMovmentTimer.Start();
				ship.ShowNodes(false);
			}
		}
	}
}
	