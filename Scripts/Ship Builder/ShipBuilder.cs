using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class ShipBuilder : Node2D
{
	[Export]
	public Button Weapon, Shield, Utility, Power;
	[Export]
	public Button SaveButton, LoadButton;
	[Export]
	private Area2D uiArea;
	[Export]
	private TextEdit modeText;
	[Export] 
	public PackedScene FileSelectScene;
	[Export] 
	public TextEdit WeaponText, ShieldText, UtilityText, PowerText;
	private List<ShipNode> shipNodes = new List<ShipNode>(); // export this to json to save ship nodes
	public enum Modes
	{
		Nodes,
		Lines,
		Colors,
	}
	public enum ShipNodeTypes
	{
		Weapon,
		Shield,
		Utility,
		Power,
	}
	private ShipNodeTypes currentNodeType = ShipNodeTypes.Weapon; // Default node type
	private Modes mode = Modes.Nodes;
	private ShipLine currentLine;
	private List<ShipLine> lines = new List<ShipLine>(); // export this to json to save ship nodes
	private List<ShipTriangle> triangles = new List<ShipTriangle>(); // export this to json to save ship nodes
	private ColorPicker colorPicker;
	private Node2D selectedTriangle;
	private bool isRightMouseHeld, dragging;
	private Node2D shadowNode;
	private bool isMouseOverUi, fileSelectMenu, uiElement;
	private ShipNode draggingNode = null;
	public int[] ResourceCount; // Resources for Weapon, Shield, Utility, Power nodes
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

		uiArea.MouseEntered += () =>
		{
			isMouseOverUi = true; // Set flag when mouse enters UI area
			GD.Print("Mouse entered UI area, hiding shadow node." + isMouseOverUi);
		};
		uiArea.MouseExited += () =>
		{
			isMouseOverUi = false; // Reset flag when mouse exits UI area
			GD.Print("Mouse exited UI area, showing shadow node." + isMouseOverUi);
		};
		Buttons(); // Initialize button actions
		LoadButtons(); // Load button actions
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (isMouseOverUi || fileSelectMenu)
		{
			uiElement = true;
		}
		else if (!isMouseOverUi && !fileSelectMenu)
		{
			uiElement = false;
		}
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
		if (dragging)
		{
			DragNode(); // Drag the node if dragging is enabled
		}
		UiStop(); // Check if mouse is over UI and update shadow node visibility
		MoveShadowNode(); // Update shadow node position
		CheckForOverlappingNodes();

		SetResourceCount();
	}

	private void SetResourceCount()
	{		
		ResourceCount = new[]
		{
			shipNodes.Count(n => n.NodeType == ShipNodeTypes.Weapon),
			shipNodes.Count(n => n.NodeType == ShipNodeTypes.Shield),
			shipNodes.Count(n => n.NodeType == ShipNodeTypes.Utility),
			shipNodes.Count(n => n.NodeType == ShipNodeTypes.Power),
		};
		WeaponText.Text = "Metal: " + ResourceCount[0] * 10;
		ShieldText.Text = "Ice: " + ResourceCount[1] * 10;
		UtilityText.Text = "Carbon: " + ResourceCount[2] * 10;
		PowerText.Text = "Gas: " + ResourceCount[3] * 10;
	}
	private void CheckForOverlappingNodes()
	{
		// Create a list to track which nodes we've already checked
		HashSet<ShipNode> checkedNodes = new HashSet<ShipNode>();

		for (int i = 0; i < shipNodes.Count; i++)
		{
			ShipNode node1 = shipNodes[i];
			if (checkedNodes.Contains(node1)) continue;

			for (int j = i + 1; j < shipNodes.Count; j++)
			{
				ShipNode node2 = shipNodes[j];
				if (checkedNodes.Contains(node2)) continue;

				// Check if the two nodes have the same position
				if (node1.GlobalPosition == node2.GlobalPosition)
				{
					// Mark node2 as checked so we don't process it again
					checkedNodes.Add(node2);

					// Call CheckNodeLocation to merge node2 into node1
					CheckNodeLocation(node2);

					// Since we modified the list, we need to adjust our loop index
					j--;
				}
			}
		}
	}
	public override void _Input(InputEvent @event)
	{
		if (uiElement) return;
		if (@event is InputEventMouseButton mouseButtonEvent)
		{
			if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
			{
				// Handle left mouse button click for ship building
				if (mode == Modes.Nodes)
				{
					PlaceNode(mouseButtonEvent.IsPressed());
				}
				else if (mode == Modes.Lines && mouseButtonEvent.IsPressed())
				{
					GD.Print("Lines mode selected, creating lines between nodes.");
					MakeLines();
				}
				else if (mode == Modes.Colors && mouseButtonEvent.IsPressed())
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
				modeText.Text = "Mode: Nodes";
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
				modeText.Text = "Mode: Lines";
				if (colorPicker != null)
				{
					colorPicker.QueueFree(); // Remove color picker if it exists
					colorPicker = null; // Reset color picker reference
				}
				GD.Print("Switched to Lines mode");
				dragging = false; // Reset dragging state when switching modes
			}
			else if (keyEvent.Keycode == Key.F3)
			{
				mode = Modes.Colors;
				modeText.Text = "Mode: Colors";
				GD.Print("Switched to Colors mode");
				dragging = false; // Reset dragging state when switching modes
			}
			else if (keyEvent.Keycode == Key.Escape)
			{
				if (currentLine != null)
				{
					currentLine.StartNode.Modulate = currentLine.StartNode.NodeColor; // Reset color of the start node
					lines.Remove(currentLine); // Remove the current line from the list
					currentLine = null; // Reset current line on Escape key
				}
				if (colorPicker != null)
				{
					colorPicker.QueueFree(); // Remove color picker if it exists
					colorPicker = null; // Reset color picker reference
				}
				dragging = false; // Reset dragging state on Escape key
			}
		}
	}
	public void PlaceNode(bool pressed)
	{
		if (uiElement) return;
		Node2D clickedObject = (Node2D)DetectClickedObject()?.GetParent();
		if (clickedObject is ShipNode || dragging)
		{
			GD.Print("Clicked on existing ShipNode: " + clickedObject.Name);
			if (pressed)
			{
				draggingNode = (ShipNode)clickedObject;
				dragging = true; // Start dragging if the node is clicked
			}
			else
			{
				if (draggingNode != null)
				{
					GD.Print("Released ShipNode: " + draggingNode.Name);
					CheckNodeLocation(draggingNode);
					draggingNode = null; // Stop dragging if the mouse button is released
				}
				dragging = false; // Stop dragging if the mouse button is released
			}
		}
		else if (pressed)
		{
			bool nodeExists = shipNodes.Any(node => node.GlobalPosition == GetGlobalMousePosition());
			if (nodeExists)
			{
				GD.Print("Node already exists at this position, not placing a new one.");
				return; // Don't place a new node if one already exists at the position
			}
			ShipNode shipNode = new ShipNode(currentNodeType);
			shipNode.GlobalPosition = new Vector2(
				Mathf.Round(GetGlobalMousePosition().X / 10) * 10,
				Mathf.Round(GetGlobalMousePosition().Y / 10) * 10
			); // Snap to grid of 10 pixels
			shipNode.Name = "ShipNode_" + shipNodes.Count;
			shipNodes.Add(shipNode);
			AddChild(shipNode);
			if (clickedObject is Line2D)
			{
				ShipLine shipLine = lines.Find(line => line.Line == clickedObject);
				ShipLine newLine = new ShipLine(shipNode, this);
				newLine.SetEndNode(shipLine.StartNode);
				lines.Add(newLine);

				newLine = new ShipLine(shipNode, this);
				newLine.SetEndNode(shipLine.EndNode);
				lines.Add(newLine);
				TriangleCheck(newLine); // Check for triangles

				draggingNode = shipNode; // Set the newly created node as the dragging node
				dragging = true; // Start dragging the newly created node
			}
		}
	}
	public void CheckNodeLocation(ShipNode node)
	{
		ShipNode overlappingNode = shipNodes.FirstOrDefault(n => n != node && n.GlobalPosition == node.GlobalPosition);

		if (overlappingNode != null)
		{
			GD.Print($"Node {node.Name} overlaps with {overlappingNode.Name}. Merging nodes...");

			// Update all lines that connect to the node being removed
			foreach (var line in lines.Where(l => l.StartNode == node || l.EndNode == node).ToList())
			{
				// Check if we already have a line between overlappingNode and the other node
				ShipNode otherNode = line.StartNode == node ? line.EndNode : line.StartNode;

				// Skip if trying to create a line from a node to itself
				if (otherNode == overlappingNode)
				{
					lines.Remove(line);
					line.Line.QueueFree();
					continue;
				}

				// Check if a line already exists between overlappingNode and otherNode
				bool lineExists = lines.Any(l =>
					(l.StartNode == overlappingNode && l.EndNode == otherNode) ||
					(l.StartNode == otherNode && l.EndNode == overlappingNode)
				);

				if (!lineExists)
				{
					// Update the line to connect to the overlapping node instead
					if (line.StartNode == node)
					{
						line.StartNode = overlappingNode;
					}
					else
					{
						line.EndNode = overlappingNode;
					}

					// Update the line's visual representation
					var points = new Vector2[2];
					points[0] = line.StartNode.GlobalPosition;
					points[1] = line.EndNode.GlobalPosition;
					line.Line.Points = points;
					line.UpdateCollisionShape();
				}
				else
				{
					// Remove duplicate line
					lines.Remove(line);
					line.Line.QueueFree();
				}
				TriangleCheck(line); // Check for triangles after updating the line
			}

			// Update all triangles that contain the node being removed
			foreach (var triangle in triangles.Where(t => t.point1 == node || t.point2 == node || t.point3 == node).ToList())
			{
				// Check if replacing the node would create a degenerate triangle (same node used twice)
				bool wouldBeDegenerate = false;
				if (triangle.point1 == node && (triangle.point2 == overlappingNode || triangle.point3 == overlappingNode))
					wouldBeDegenerate = true;
				else if (triangle.point2 == node && (triangle.point1 == overlappingNode || triangle.point3 == overlappingNode))
					wouldBeDegenerate = true;
				else if (triangle.point3 == node && (triangle.point1 == overlappingNode || triangle.point2 == overlappingNode))
					wouldBeDegenerate = true;

				if (wouldBeDegenerate)
				{
					// Remove the triangle as it would become degenerate
					triangles.Remove(triangle);
					triangle.TriangleNode.QueueFree();
				}
				else
				{
					// Update the triangle to use the overlapping node
					if (triangle.point1 == node)
						triangle.point1 = overlappingNode;
					else if (triangle.point2 == node)
						triangle.point2 = overlappingNode;
					else if (triangle.point3 == node)
						triangle.point3 = overlappingNode;

					// Update the triangle's visual representation
					triangle.UpdateTriangle();
				}
			}

			// Update connected nodes lists
			foreach (var connectedNode in node.ConnectedNodes)
			{
				if (connectedNode != overlappingNode)
				{
					// Remove the old node from connected nodes
					connectedNode.ConnectedNodes.Remove(node);

					// Add the overlapping node if not already connected
					if (!connectedNode.ConnectedNodes.Contains(overlappingNode))
					{
						connectedNode.ConnectedNodes.Add(overlappingNode);
					}

					// Add this node to overlapping node's connections if not already there
					if (!overlappingNode.ConnectedNodes.Contains(connectedNode))
					{
						overlappingNode.ConnectedNodes.Add(connectedNode);
					}
				}
			}

			// Remove the node from the scene and from the list
			shipNodes.Remove(node);
			node.QueueFree();
			GD.Print($"Node {node.Name} has been merged into {overlappingNode.Name}");
		}
	}
	private void DragNode()
	{
		if (draggingNode != null)
		{
			Vector2 newPosition = new Vector2(
				Mathf.Round(GetGlobalMousePosition().X / 10) * 10,
				Mathf.Round(GetGlobalMousePosition().Y / 10) * 10
			); // Snap to grid of 10 pixels
			draggingNode.GlobalPosition = newPosition;
			GD.Print("Dragging ShipNode: " + draggingNode.Name + " to position: " + newPosition);

			lines.ForEach(line =>
			{
				if (line.StartNode == draggingNode || line.EndNode == draggingNode)
				{
					// Create a new points array and reassign it
					var points = new Vector2[2];
					points[0] = line.StartNode.GlobalPosition;
					points[1] = line.EndNode.GlobalPosition;
					line.Line.Points = points;
					line.UpdateCollisionShape();
				}
			});

			triangles.ForEach(triangle =>
			{
				if (triangle.point1 == draggingNode || triangle.point2 == draggingNode || triangle.point3 == draggingNode)
				{
					// Update triangle position
					triangle.UpdateTriangle();
				}
			});
		}
	}
	private void MakeLines()
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
						currentLine.StartNode.Modulate = currentLine.StartNode.NodeColor; // Reset color of the start node
						currentLine = null; // Reset current line
						return;
					}
				});
				if (clickedShipNode == currentLine.StartNode)
				{
					GD.Print("Clicked on the start node, resetting current line.");
					currentLine.StartNode.Modulate = currentLine.StartNode.NodeColor; // Reset color of the start node
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
	private void UiStop()
	{
		if (uiElement)
		{
			shadowNode.Visible = false; // Hide shadow node if mouse is over UI
		}
		else if (mode == Modes.Nodes)
		{
			shadowNode.Visible = true; // Show shadow node if in Nodes mode
		}
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

			if (collider is Node2D hitObject)
			{
				if (buttonIndex == 1 && mode == Modes.Nodes && hitObject.GetParent() is ShipNode)
				{
					GD.Print("Hit Node: " + hitObject.Name);
					return hitObject;
				}
				else if (buttonIndex == 1 && mode == Modes.Nodes && hitObject.GetParent() is Line2D)
				{
					if (shapeResults.Any(s => s["collider"] is ShipNode))
					{
						GD.Print("Hit Line but also hit ShipNode, ignoring Line2D: " + hitObject.Name);
						continue;
					}
					GD.Print("Hit Line: " + hitObject.Name);
					return hitObject;
				}
				else if (buttonIndex == 1 && mode == Modes.Lines && hitObject.GetParent() is ShipNode)
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
	private void TriangleCheck(ShipLine line)
	{
		foreach (ShipNode node in line.StartNode.ConnectedNodes)
		{
			if (line.EndNode.ConnectedNodes.Contains(node))
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
	private void PickColor()
	{
		Node2D clickedObject = (Node2D)DetectClickedObject()?.GetParent();
		if (clickedObject is not ShipNode)
		{
			if (clickedObject != selectedTriangle || colorPicker == null)
			{
				GD.Print("Picked color for ShipNode: " + clickedObject.Name);
				colorPicker?.QueueFree(); // Remove the previous color picker if it exists
				colorPicker = new ColorPicker()
				{
					SamplerVisible = false,
					ColorModesVisible = false,
					SlidersVisible = false,
					HexVisible = false,
					PresetsVisible = false,
				};
				colorPicker.Position = new Vector2(0, GetViewportRect().Size.Y - 300);
				colorPicker.CustomMinimumSize = new Vector2(300, 300);
				colorPicker.AnchorBottom = 1;
				colorPicker.AnchorLeft = 0;
				
				selectedTriangle = clickedObject;
				AddChild(colorPicker);
				// Implement color picking logic here
			}
		}
	}
	private void RemoveObject()
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
				var removeNodes = shipNodes.Where(n => n.ConnectedNodes.Contains(shipNode)).ToList();
				foreach (var n in removeNodes)
				{
					n.ConnectedNodes.Remove(shipNode);
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
					shipLine.StartNode.ConnectedNodes.Remove(shipLine.EndNode);
					shipLine.EndNode.ConnectedNodes.Remove(shipLine.StartNode);

					var trianglesWithLine = triangles.Where(t => t.lines.Contains(shipLine)).ToList();
					foreach (var t in trianglesWithLine)
					{
						triangles.Remove(t);
						t.TriangleNode.QueueFree(); // Remove the triangle from the scene
					}
					currentLine = null; // Reset the current line if it was the one being edited
										// Remove the line from any triangles that contain it
				}
			}
			else
			{
				GD.Print("Clicked on an unhandled object: " + clickedObject.Name);
			}
		}
	}
	private void MoveShadowNode()
	{
		if (mode != Modes.Nodes || uiElement)
		{
			shadowNode.Visible = false; // Hide shadow node if not in Nodes mode
		}
		else
		{
			shadowNode.Visible = true; // Show shadow node if in Nodes mode
			shadowNode.Modulate = currentNodeType switch
			{
				ShipNodeTypes.Weapon => new Color(1, 0.5f, 0.5f, 0.5f), // Light Red for weapon nodes
				ShipNodeTypes.Utility => new Color(0.5f, 1, 0.5f, 0.5f), // Light Green for utility nodes
				ShipNodeTypes.Shield => new Color(0.5f, 0.5f, 1, 0.5f), // Light Blue for shield nodes
				ShipNodeTypes.Power => new Color(1, 1, 0, 0.5f), // Light Yellow for power nodes
				_ => new Color(1, 1, 1, 0.5f) // Default color for other types
			};
			shadowNode.GlobalPosition = new Vector2(
				Mathf.Round(GetGlobalMousePosition().X / 10) * 10,
				Mathf.Round(GetGlobalMousePosition().Y / 10) * 10
			); // Snap to grid of 10 pixels
		}
	}
	private void Buttons()
	{
		Weapon.ButtonDown += () =>
		{
			currentNodeType = ShipNodeTypes.Weapon;
			GD.Print("Selected Weapon node type");
		};
		Shield.ButtonDown += () =>
		{
			currentNodeType = ShipNodeTypes.Shield;
			GD.Print("Selected Shield node type");
		};
		Utility.ButtonDown += () =>
		{
			currentNodeType = ShipNodeTypes.Utility;
			GD.Print("Selected Utility node type");
		};
		Power.ButtonDown += () =>
		{
			currentNodeType = ShipNodeTypes.Power;
			GD.Print("Selected Power node type");
		};
	}
	private void LoadButtons()
	{
		SaveButton.ButtonDown += () =>
		{
			GD.Print("Save button pressed");
			SelectFile sf = FileSelectScene.Instantiate<SelectFile>();
			sf.Position = Vector2.Zero;
			GetChildren()
				.OfType<CanvasLayer>()
				.ToList()
				.ForEach(canvasLayer => canvasLayer.Visible = false);
			CanvasLayer topLayer = new CanvasLayer();
			topLayer.Layer = 100;  // High layer value
			AddChild(topLayer);
			topLayer.AddChild(sf);
			sf.Save = true;
			fileSelectMenu = true;
			sf.Select.ButtonDown += () =>
			{
				SaveShip(sf.SavePath.Text == "" ? sf.MyShipPath : sf.SavePath.Text);
				fileSelectMenu = true;
				sf.QueueFree();
				GetChildren()
					.OfType<CanvasLayer>() 
					.ToList()
					.ForEach(canvasLayer => canvasLayer.Visible = true);
				topLayer.QueueFree();
			};

		};
		LoadButton.ButtonDown += () =>
		{
			GD.Print("Load button pressed");
			SelectFile sf = FileSelectScene.Instantiate<SelectFile>();
			sf.Position = Vector2.Zero;
			GetChildren()
				.OfType<CanvasLayer>()
				.ToList()
				.ForEach(canvasLayer => canvasLayer.Visible = false);
			CanvasLayer topLayer = new CanvasLayer();
			topLayer.Layer = 100;  // High layer value
			AddChild(topLayer);
			topLayer.AddChild(sf);

			sf.Save = false;
			fileSelectMenu = true;
			sf.Select.ButtonDown += () =>
			{
				LoadShip(sf.SelectedFilePath);
				fileSelectMenu = true;
				sf.QueueFree();
				GetChildren()
					.OfType<CanvasLayer>() 
					.ToList()
					.ForEach(canvasLayer => canvasLayer.Visible = true);
				topLayer.QueueFree();
			};
		};
	}
	private void SaveShip(string path)
	{
		if (path == "") return;
		
		ShipSave shipSave = new ShipSave();
		shipSave.NodePositions = new Godot.Collections.Array<Vector2>();
		shipSave.NodeTypes = new Godot.Collections.Array<int>();
		shipSave.DefineTriangles = new Godot.Collections.Array<int>();
		shipSave.TriangleColors = new Godot.Collections.Array<Color>();

		foreach (var node in shipNodes)
		{
			shipSave.NodePositions.Add(node.GlobalPosition);
			shipSave.NodeTypes.Add((int)node.NodeType);
		}

		foreach (var triangle in triangles)
		{
			shipSave.DefineTriangles.Add(shipNodes.IndexOf(triangle.point1));
			shipSave.DefineTriangles.Add(shipNodes.IndexOf(triangle.point2));
			shipSave.DefineTriangles.Add(shipNodes.IndexOf(triangle.point3));
			shipSave.TriangleColors.Add(triangle.TriangleNode.Modulate);
		}
		shipSave.Lines = new Godot.Collections.Array<Vector2I>();
		foreach (var line in lines)
		{
			int startIdx = shipNodes.IndexOf(line.StartNode);
			int endIdx = shipNodes.IndexOf(line.EndNode);
			shipSave.Lines.Add(new Vector2I(startIdx, endIdx));
		}
		var rc = new Godot.Collections.Array<int>();
		rc.AddRange(ResourceCount);
		shipSave.ResourceCounts = rc;
		if (!path.Contains('\\'))
		{
			path = $"MyShips\\{path}.tres";
		}
		ResourceSaver.Save(shipSave, $"{path}.tres");
		GD.Print("Ship saved successfully.");
	}
	private void LoadShip(string path)
	{
		ShipSave shipSave = (ShipSave)ResourceLoader.Load($"{path}");
		if (shipSave == null)
		{
			GD.Print("Failed to load ship save.");
			return;
		}

		// Clear existing nodes and lines
		foreach (var node in shipNodes)
		{
			node.QueueFree();
		}
		shipNodes.Clear();

		foreach (var line in lines)
		{
			line.Line.QueueFree();
		}
		lines.Clear();

		foreach (var triangle in triangles)
		{
			triangle.TriangleNode.QueueFree();
		}
		triangles.Clear();

		// Load nodes
		for (int i = 0; i < shipSave.NodePositions.Count; i++)
		{
			var node = new ShipNode((ShipNodeTypes)shipSave.NodeTypes[i]);
			node.GlobalPosition = shipSave.NodePositions[i];
			node.Name = "ShipNode_" + i;
			AddChild(node);
			shipNodes.Add(node);
			GD.Print("Loaded ShipNode: " + node.Name + " at position: " + node.GlobalPosition);
		}

		// Load lines
		foreach (var linePair in shipSave.Lines)
		{
			var startNode = shipNodes[linePair.X];
			var endNode = shipNodes[linePair.Y];
			var line = new ShipLine(startNode, this);
			line.SetEndNode(endNode);
			lines.Add(line);
		}

		// Now rebuild all triangles based on the lines
		RebuildAllTriangles();
		
		for (var index = 0; index < triangles.Count; index++)
		{
			var triangle = triangles[index];
			if (shipSave.TriangleColors.Count <= index) continue;
			triangle.TriangleNode.Modulate = shipSave.TriangleColors[index];
			foreach (var line in triangle.lines)
			{
				var pickedColor = shipSave.TriangleColors[index];
				bool isDark = pickedColor is { R: < 0.2f, G: < 0.2f, B: < 0.2f };
				line.Line.Modulate = !isDark ? pickedColor : new Color(0.5f, 0.5f, 0.5f, 1);
			}
		}
	}
	private void RebuildAllTriangles()
	{
		// Remove all existing triangles
		foreach (var triangle in triangles)
		{
			triangle.TriangleNode.QueueFree();
		}

		triangles.Clear();

		// Check all combinations of three nodes
		for (int i = 0; i < shipNodes.Count; i++)
		{
			for (int j = i + 1; j < shipNodes.Count; j++)
			{
				for (int k = j + 1; k < shipNodes.Count; k++)
				{
					ShipNode a = shipNodes[i];
					ShipNode b = shipNodes[j];
					ShipNode c = shipNodes[k];

					// Check if all three lines exist between these nodes
					bool ab = lines.Any(l =>
						(l.StartNode == a && l.EndNode == b) || (l.StartNode == b && l.EndNode == a));
					bool bc = lines.Any(l =>
						(l.StartNode == b && l.EndNode == c) || (l.StartNode == c && l.EndNode == b));
					bool ca = lines.Any(l =>
						(l.StartNode == c && l.EndNode == a) || (l.StartNode == a && l.EndNode == c));

					if (!ab || !bc || !ca) continue;
					
					// Avoid duplicate triangles
					bool triangleExists = triangles.Any(t =>
						(t.point1 == a || t.point2 == a || t.point3 == a) &&
						(t.point1 == b || t.point2 == b || t.point3 == b) &&
						(t.point1 == c || t.point2 == c || t.point3 == c)
					);
					if (triangleExists) continue;
					var triangleLines = new List<ShipLine>
					{
						lines.Find(l =>
							(l.StartNode == a && l.EndNode == b) || (l.StartNode == b && l.EndNode == a)),
						lines.Find(l =>
							(l.StartNode == b && l.EndNode == c) || (l.StartNode == c && l.EndNode == b)),
						lines.Find(l =>
							(l.StartNode == c && l.EndNode == a) || (l.StartNode == a && l.EndNode == c))
					};
					ShipTriangle triangle = new ShipTriangle(a, b, c, this, triangleLines);
					triangles.Add(triangle);
				}
			}
		}
	}
}