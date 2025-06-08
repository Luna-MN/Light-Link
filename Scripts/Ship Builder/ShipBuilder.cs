using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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
	public List<ShipTriangle> triangles = new List<ShipTriangle>();
	public ColorPicker colorPicker;
	public Node2D selectedTriangle;
	public bool isRightMouseHeld = false;
	public Node2D shadowNode;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		shadowNode = new Node2D();
		shadowNode.Name = "ShadowNode";
		shadowNode.GlobalPosition = GetGlobalMousePosition();
		AddChild(shadowNode);

		MeshInstance2D shadowMesh = new MeshInstance2D();
		shadowMesh.Mesh = new SphereMesh(); // Example mesh, replace with your shadow mesh
		shadowMesh.Scale = new Vector2(10f, 10f); // Scale the mesh to a reasonable size
		shadowNode.AddChild(shadowMesh);
		shadowNode.ZIndex = 1000; // Ensure the shadow node is drawn above other nodes
		shadowNode.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Semi-transparent shadow color

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (colorPicker != null)
		{
			selectedTriangle.Modulate = colorPicker.Color;
			var triangle = triangles.Find(t => t.TriangleNode == selectedTriangle);
			if (triangle != null)
			{
				Color pickedColor = colorPicker.Color;
				bool isDark = pickedColor.R < 0.2f && pickedColor.G < 0.2f && pickedColor.B < 0.2f;
				Color lineColor = isDark ? Colors.White : pickedColor;
				foreach (var line in triangle.lines)
				{
					line.Line.DefaultColor = lineColor;
				}
			}
		}
		if (isRightMouseHeld)
		{
			RemoveObject();
		}
		MoveShadowNode(); // Update shadow node position based on mouse position
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
					PickColor();
				}
				// Add your ship building logic here
			}

		}
		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.Right)
			{
				if (mouseButton.IsPressed())
				{
					isRightMouseHeld = true;
				}
				else
				{
					isRightMouseHeld = false;
				}
			}
		}
		if (@event is InputEventKey keyEvent && keyEvent.IsPressed())
		{
			if (keyEvent.Keycode == Key.F1)
			{
				mode = Modes.Nodes;
				if (colorPicker != null)
				{
					colorPicker.QueueFree(); // Remove color picker if it exists
					colorPicker = null; // Reset color picker reference{

				}
				GD.Print("Switched to Nodes mode");
			}
			else if (keyEvent.Keycode == Key.F2)
			{
				mode = Modes.Lines;
				if (colorPicker != null)
				{
					colorPicker.QueueFree(); // Remove color picker if it exists
					colorPicker = null; // Reset color picker reference
				}
				GD.Print("Switched to Lines mode");
			}
			else if (keyEvent.Keycode == Key.F3)
			{
				mode = Modes.Colors;
				GD.Print("Switched to Colors mode");
			}
			else if (keyEvent.Keycode == Key.Escape)
			{
				if (currentLine != null)
				{
					currentLine.StartNode.Modulate = new Color(1, 1, 1); // Reset color of the start node
					lines.Remove(currentLine); // Remove the current line from the list
					currentLine = null; // Reset current line on Escape key
				}
				if (colorPicker != null)
				{
					colorPicker.QueueFree(); // Remove color picker if it exists
					colorPicker = null; // Reset color picker reference
				}
			}
		}
	}
	public void PlaceNode()
	{
		ShipNode shipNode = new ShipNode();
		shipNode.GlobalPosition = new Vector2(
			Mathf.Round(GetGlobalMousePosition().X / 10) * 10,
			Mathf.Round(GetGlobalMousePosition().Y / 10) * 10
		); // Snap to grid of 10 pixels
		shipNode.Name = "ShipNode_" + shipNodes.Count;
		shipNodes.Add(shipNode);
		AddChild(shipNode);
	}
	public void MakeLines()
	{
		Node2D clickedObject = (Node2D)DetectClickedObject()?.GetParent();
		GD.Print("Current Line: " + currentLine?.StartNode?.Name);
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
				lines.ForEach(line =>
				{
					if (line.StartNode == clickedShipNode && line.EndNode == currentLine.StartNode ||
						line.EndNode == clickedShipNode && line.StartNode == currentLine.StartNode)
					{
						// If a line already exists between the clicked node and the current line's start node
						GD.Print("Line already exists with this node, resetting current line.");
						currentLine.StartNode.Modulate = new Color(1, 1, 1); // Reset color of the start node
						currentLine = null; // Reset current line
						return;
					}
				});
				if (clickedShipNode == currentLine.StartNode)
				{
					GD.Print("Clicked on the start node, resetting current line.");
					currentLine.StartNode.Modulate = new Color(1, 1, 1); // Reset color of the start node
					currentLine = null; // Reset current line
					return;
				}
				// Update the end point of the current line
				currentLine.SetEndNode(clickedShipNode);
				TriangleCheck(currentLine); // Check for triangles
				currentLine = null;
				if (Input.IsKeyPressed(Key.Ctrl))
				{
					currentLine = new ShipLine(clickedShipNode, this); // Reset for the next line
					lines.Add(currentLine);
				}
			}
		}
		GD.Print("Lines count: " + lines.Count);
	}
	private Node2D DetectClickedObject(int buttonIndex = 1)
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
				if (buttonIndex == 1 && mode == Modes.Lines && hitObject.GetParent() is ShipNode)
				{
					GD.Print("Hit Node: " + hitObject.Name);
					return hitObject;
				}
				else if (mode != Modes.Lines || buttonIndex == 2)
				{
					GD.Print("Hit object: " + hitObject.Name);
					return hitObject;
				}
				else if (buttonIndex == 2 && !hitObject.GetParent().Name.ToString().Contains("ShipTriangle"))
				{
					GD.Print("Didn't hit ShipTriangle: " + hitObject.Name);
					return hitObject;
				}
			}
		}
		return null;
	}
	public void TriangleCheck(ShipLine line)
	{
		foreach (ShipNode node in line.StartNode.connectedNodes)
		{
			if (line.EndNode.connectedNodes.Contains(node))
			{
				// We have a triangle, so we need to remove the line
				GD.Print("Triangle detected");
				List<ShipLine> triangleLines = new List<ShipLine>
				{
					line,
					lines.Find(l =>
						(l.StartNode == line.StartNode && l.EndNode == node) ||
						(l.StartNode == node && l.EndNode == line.StartNode)
					),
					lines.Find(l =>
						(l.StartNode == node && l.EndNode == line.EndNode) ||
						(l.StartNode == line.EndNode && l.EndNode == node)
					)
				};
				ShipTriangle triangle = new ShipTriangle(line.StartNode, line.EndNode, node, this, triangleLines);
				triangles.Add(triangle);
				return;

			}
		}
	}
	public void PickColor()
	{
		Node2D clickedObject = (Node2D)DetectClickedObject()?.GetParent();
		if (clickedObject is not ShipNode)
		{
			if (clickedObject != selectedTriangle || colorPicker == null)
			{
				GD.Print("Picked color for ShipNode: " + clickedObject.Name);
				colorPicker?.QueueFree(); // Remove previous color picker if it exists
				colorPicker = new ColorPicker();
				selectedTriangle = clickedObject;
				AddChild(colorPicker);
				// Implement color picking logic here
			}
		}
	}
	public void RemoveObject()
	{
		Node2D clickedObject = (Node2D)DetectClickedObject(2)?.GetParent();
		if (clickedObject != null)
		{
			// Handle right mouse button click for removing nodes or lines
			if (clickedObject is ShipNode shipNode)
			{
				shipNodes.Remove(shipNode);
				GD.Print("Removed ShipNode: " + shipNode.Name);

				// Remove all lines connected to this node
				var linesToRemove = lines.Where(line => line.StartNode == shipNode || line.EndNode == shipNode).ToList();
				var removeNodes = shipNodes.Where(n => n.connectedNodes.Contains(shipNode)).ToList();
				foreach (var n in removeNodes)
				{
					n.connectedNodes.Remove(shipNode);
				}
				foreach (var line in linesToRemove)
				{
					lines.Remove(line);
					line.Line.QueueFree(); // Remove the line from the scene
				}

				// Remove all triangles containing this node
				var trianglesToRemove = triangles.Where(t => t.point1 == shipNode || t.point2 == shipNode || t.point3 == shipNode).ToList();
				foreach (var t in trianglesToRemove)
				{
					triangles.Remove(t);
					t.TriangleNode.QueueFree(); // Remove the triangle from the scene
				}

				shipNode.QueueFree(); // Remove the node from the scene
			}
			// Handle right-click on lines
			else if (clickedObject is Line2D line2D)
			{
				GD.Print("Clicked on Line2D: " + line2D.Name);
				ShipLine shipLine = lines.Find(l => l.Line == line2D);
				GD.Print("Found ShipLine: " + shipLine?.StartNode?.Name + " to " + shipLine?.EndNode?.Name);
				if (shipLine != null)
				{
					lines.Remove(shipLine);
					shipLine.Line.QueueFree(); // Remove the line from the scene
					shipLine.StartNode.connectedNodes.Remove(shipLine.EndNode);
					shipLine.EndNode.connectedNodes.Remove(shipLine.StartNode);

					var trianglesWithLine = triangles.Where(t => t.lines.Contains(shipLine)).ToList();
					foreach (var t in trianglesWithLine)
					{
						triangles.Remove(t);
						t.TriangleNode.QueueFree(); // Remove the triangle from the scene
					}
					currentLine = null; // Reset current line if it was the one being edited
										// Remove the line from any triangles that contain it
				}
			}
			else
			{
				GD.Print("Clicked on an unhandled object: " + clickedObject.Name);
			}
		}
	}
	public void MoveShadowNode()
	{
		if (mode != Modes.Nodes)
		{
			shadowNode.Visible = false; // Hide shadow node if not in Nodes mode
		}
		else
		{
			shadowNode.Visible = true; // Show shadow node if in Nodes mode
			shadowNode.GlobalPosition = new Vector2(
				Mathf.Round(GetGlobalMousePosition().X / 10) * 10,
				Mathf.Round(GetGlobalMousePosition().Y / 10) * 10
			); // Snap to grid of 10 pixels
		}
	}
}
