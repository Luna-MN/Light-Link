using Godot;
using System;
using System.Collections.Generic;

public partial class ShipBuilder : Node2D
{
	public List<ShipNode> shipNodes = new List<ShipNode>();
	public enum Modes
	{
		Nodes,
		Lines,
		Colors,
	}
	public Modes mode = Modes.Nodes;
	public ShipLine currentLine;
	public List<ShipLine> lines = new List<ShipLine>();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.IsPressed())
		{
			if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
			{
				// Handle left mouse button click for ship building
				if (mode == Modes.Nodes)
				{
					PlaceNode();
				}
				else if (mode == Modes.Lines)
				{
					GD.Print("Lines mode selected, creating lines between nodes.");
					MakeLines();

				}
				else if (mode == Modes.Colors)
				{
					GD.Print("Colors mode selected, but no action defined yet.");
				}
				// Add your ship building logic here
			}
		}
		if (@event is InputEventKey keyEvent && keyEvent.IsPressed())
		{
			if (keyEvent.Keycode == Key.F1)
			{
				mode = Modes.Nodes;
				GD.Print("Switched to Nodes mode");
			}
			else if (keyEvent.Keycode == Key.F2)
			{
				mode = Modes.Lines;
				GD.Print("Switched to Lines mode");
			}
			else if (keyEvent.Keycode == Key.F3)
			{
				mode = Modes.Colors;
				GD.Print("Switched to Colors mode");
			}
		}
	}
	public void PlaceNode()
	{
		ShipNode shipNode = new ShipNode();
		shipNode.GlobalPosition = GetGlobalMousePosition();
		shipNode.Name = "ShipNode_" + shipNodes.Count;
		shipNodes.Add(shipNode);
		AddChild(shipNode);
	}
	public void MakeLines()
	{
		Node2D clickedObject = (Node2D)DetectClickedObject()?.GetParent();
		GD.Print("Clicked Object: " + clickedObject?.Name);

		if (clickedObject is ShipNode clickedShipNode)
		{
			if (currentLine == null)
			{
				// Start a new line
				currentLine = new ShipLine(clickedShipNode, this);
				lines.Add(currentLine);
			}
			else
			{
				// Update the end point of the current line
				currentLine.SetEndNode(clickedShipNode);
				currentLine = null; // Reset for the next line
			}
		}
	}
	private Node2D DetectClickedObject()
	{
		// Convert to global position
		Vector2 globalPos = GetGlobalMousePosition();

		var spaceState = GetWorld2D().DirectSpaceState;

		// Cast rays in multiple directions to be more thorough
		var queryParams = new PhysicsRayQueryParameters2D();
		queryParams.From = globalPos;
		queryParams.To = globalPos; // Same position for point query
		queryParams.CollideWithAreas = true;
		queryParams.CollideWithBodies = true;
		queryParams.HitFromInside = true; // This is important to detect if we're inside an area

		// Try a shape cast instead of a ray cast
		var shape = new CircleShape2D();
		shape.Radius = 5.0f; // Small radius to detect nearby objects

		var shapeQuery = new PhysicsShapeQueryParameters2D();
		shapeQuery.Shape = shape;
		shapeQuery.Transform = new Transform2D(0, globalPos);
		shapeQuery.CollideWithAreas = true;
		shapeQuery.CollideWithBodies = true;

		var shapeResults = spaceState.IntersectShape(shapeQuery);
		foreach (var result in shapeResults)
		{

			GodotObject collider = result["collider"].As<GodotObject>();

			Node2D hitObject = collider as Node2D;
			if (hitObject != null)
			{
				GD.Print("Hit ShipNode: " + hitObject.Name);
				return hitObject;
			}
		}
		return null;
	}
}
