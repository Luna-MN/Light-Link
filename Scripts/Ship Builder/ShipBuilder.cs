using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
[GlobalClass]
public partial class ShipBuilder : Node2D
{
	[Export]
	public Button Weapon, Shield, Utility, Power;
	[Export]
	public Button SaveButton, LoadButton;
	[Export] 
	public PackedScene FileSelectScene;
	[Export] 
	public TextEdit WeaponText, ShieldText, UtilityText, PowerText;
	[Export]
	public Button NodeButton, LineButton, TriangleButton;
	[Export]
	public Node2D NodesParent, LinesParent, TrianglesParent;
	[Export]
	public CanvasLayer canvasLayer;
	[Export] 
	public Button MirrorXB, MirrorYB;
	private bool MirrorX = false, MirrorY = false;
	private bool nodeVis = true, lineVis = true, triangleVis = true;
	public int GridSize = 20;
	private List<ShipNode> shipNodes = new List<ShipNode>(); // export this to json to save ship nodes
	private bool isMouseDown, isPressed, createdLast;
	private float mouseDownTime, dragStartDelay = 0.2f;
	private bool placing = true; 
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
		Start
	}
	private ShipNodeTypes currentNodeType = ShipNodeTypes.Weapon; // Default node type
	private ShipLine currentLine;
	private List<ShipLine> lines = new List<ShipLine>(); // export this to json to save ship nodes
	private List<ShipTriangle> triangles = new List<ShipTriangle>(); // export this to json to save ship nodes
	private ColorPicker colorPicker;
	private Node2D selectedTriangle;
	private bool isRightMouseHeld, dragging;
	private Node2D shadowNode, mirrorNodeX, mirrorNodeY, mirrorNodeXY;
	private bool isMouseOverUi, fileSelectMenu, uiElement;
	private ShipNode draggingNode = null;
	private Node2D shipStartNode;
	public int[] ResourceCount; // Resources for Weapon, Shield, Utility, Power 
	private bool PlaceStart = true;

	private Timer BlockInput;
	private bool blocked;
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
		shadowNode.Visible = false;
		placing = false;
		
		mirrorNodeX = new Node2D();
		mirrorNodeX.Name = "MirrorNodeX";
		mirrorNodeX.GlobalPosition = GetGlobalMousePosition();
		AddChild(mirrorNodeX);
		
		MeshInstance2D mirrorMeshX = new MeshInstance2D();
		mirrorMeshX.Mesh = new SphereMesh(); // Example mesh, replace with your shadow mesh
		mirrorMeshX.Scale = new Vector2(10f, 10f); // Scale the mesh to a reasonable size
		mirrorNodeX.AddChild(mirrorMeshX);
		mirrorNodeX.ZIndex = 1000; // Ensure the shadow node is drawn above other nodes
		mirrorNodeX.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Semi-transparent shadow color
		mirrorNodeX.Visible = false;
		
		mirrorNodeY = new Node2D();
		mirrorNodeY.Name = "MirrorNodeY";
		mirrorNodeY.GlobalPosition = GetGlobalMousePosition();
		AddChild(mirrorNodeY);
		
		MeshInstance2D mirrorMeshY = new MeshInstance2D();
		mirrorMeshY.Mesh = new SphereMesh(); // Example mesh, replace with your shadow mesh
		mirrorMeshY.Scale = new Vector2(10f, 10f); // Scale the mesh to a reasonable size
		mirrorNodeY.AddChild(mirrorMeshY);
		mirrorNodeY.ZIndex = 1000; // Ensure the shadow node is drawn above other nodes
		mirrorNodeY.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Semi-transparent shadow color
		mirrorNodeY.Visible = false;
		
		mirrorNodeXY = new Node2D();
		mirrorNodeXY.Name = "MirrorNodeXY";
		mirrorNodeXY.GlobalPosition = GetGlobalMousePosition();
		AddChild(mirrorNodeXY);
		
		MeshInstance2D mirrorMeshXY = new MeshInstance2D();
		mirrorMeshXY.Mesh = new SphereMesh(); // Example mesh, replace with your shadow mesh
		mirrorMeshXY.Scale = new Vector2(10f, 10f); // Scale the mesh to a reasonable size
		mirrorNodeXY.AddChild(mirrorMeshXY);
		mirrorNodeXY.ZIndex = 1000; // Ensure the shadow node is drawn above other nodes
		mirrorNodeXY.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Semi-transparent shadow color
		mirrorNodeXY.Visible = false;
		
		var ShipStartScene = GD.Load<PackedScene>("res://Scripts/Ship Builder/ShipStartNode.tscn");
		shipStartNode = ShipStartScene.Instantiate<Node2D>();
		AddChild(shipStartNode);
		shipStartNode.Name = "ShipStartNode";
		shipStartNode.Position = GetGlobalMousePosition();
		shipNodes.Add(shipStartNode.GetChildren().FirstOrDefault(x => x.Name == "ShipStartNode") as ShipStartNode);
		shipNodes.Add(shipStartNode.GetChildren().FirstOrDefault(x => x.Name == "ShipStartNode2") as ShipStartNode);
		lines.Add(shipStartNode.GetChildren().FirstOrDefault(x => x.Name == "Line2D") as ShipLine);

		BlockInput = new Timer()
		{
			Autostart = false,
			OneShot = true,
			WaitTime = 0.5f
		};
		BlockInput.Timeout += () =>
		{
			blocked = false;
		};
		AddChild(BlockInput);
		MirrorYB.ButtonDown += () =>
		{
			MirrorY = !MirrorY;
			MirrorYB.Modulate = MirrorYB.Modulate == Colors.White ? Colors.LightBlue : Colors.White;
		};
		MirrorXB.ButtonDown += () =>
		{
			MirrorX = !MirrorX;
			MirrorXB.Modulate = MirrorXB.Modulate == Colors.White ? Colors.LightBlue : Colors.White;
		};
		Buttons(); // Initialize button actions
		LoadButtons(); // Load button actions
		VisibilityButtons();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (PlaceStart)
		{
			shipStartNode.GlobalPosition = new Vector2(
				Math.Clamp(Mathf.Round(GetGlobalMousePosition().X / GridSize) * GridSize, 0, 2000),
				Math.Clamp(Mathf.Round(GetGlobalMousePosition().Y / GridSize) * GridSize, 0, 2000)
			);
		}
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
			var triangle = triangles.Find(t => t == selectedTriangle);
			if (triangle != null)
			{
				Color pickedColor = colorPicker.Color;
				bool isDark = pickedColor.R < 0.2f && pickedColor.G < 0.2f && pickedColor.B < 0.2f;
				Color lineColor = isDark ? Colors.White : pickedColor;
				foreach (var line in triangle.lines)
				{
					line.DefaultColor = lineColor;
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

		if (!Input.IsMouseButtonPressed(MouseButton.Left))
		{
			dragging = false;
		}
		MoveShadowNode(); // Update shadow node position
		MoveMirrorNode();
		CheckForOverlappingNodes();
		SetResourceCount();
		if (currentLine != null && currentLine.Points.Length != 2)
		{
			currentLine.LineFollowMouse();
		}
		else if (currentLine != null && currentLine.Points.Length == 2)
		{
			currentLine.LineToMose();
		}

		if (isMouseDown)
		{
			mouseDownTime += (float)delta;

			if (mouseDownTime > 0.5f && !dragging)
			{
				isPressed = true;
				if (isPressed)
				{
					Node2D clickedObject = (Node2D)DetectClickedObject()?.GetParent();
					if (clickedObject is ShipNode node)
					{
						draggingNode = node;
						dragging = true; // Start dragging if the node is clicked
					}

				}

			}
		}
		shipNodes.ForEach(n => n.Visible = nodeVis);
		lines.ForEach(l => l.Visible = lineVis);
		triangles.ForEach(t => t.Visible = triangleVis);
	}

	private void MoveMirrorNode()
	{
		if (MirrorY)
		{
			mirrorNodeY.Visible = true;

			var startY = shipStartNode.GlobalPosition.Y;
			var shadow = shadowNode.GlobalPosition;

			mirrorNodeY.GlobalPosition = new Vector2(
				shadow.X,
				2 * startY - shadow.Y
			);
			
			mirrorNodeY.Modulate = currentNodeType switch
			{
				ShipNodeTypes.Weapon => new Color(1, 0.5f, 0.5f, 0.5f), // Light Red for weapon nodes
				ShipNodeTypes.Utility => new Color(0.5f, 1, 0.5f, 0.5f), // Light Green for utility nodes
				ShipNodeTypes.Shield => new Color(0.5f, 0.5f, 1, 0.5f), // Light Blue for shield nodes
				ShipNodeTypes.Power => new Color(1, 1, 0, 0.5f), // Light Yellow for power nodes
				_ => new Color(1, 1, 1, 0.5f) // Default color for other types
			};
		}
		else
		{
			mirrorNodeY.Visible = false;
		}
		if (MirrorX)
		{
			mirrorNodeX.Visible = true;
			
			var startX = shipStartNode.GlobalPosition.X;
			var shadow = shadowNode.GlobalPosition;
			
			mirrorNodeX.GlobalPosition = new Vector2(
				2 * startX - shadow.X,
				shadow.Y
			);
			
			mirrorNodeX.Modulate = currentNodeType switch
			{
				ShipNodeTypes.Weapon => new Color(1, 0.5f, 0.5f, 0.5f), // Light Red for weapon nodes
				ShipNodeTypes.Utility => new Color(0.5f, 1, 0.5f, 0.5f), // Light Green for utility nodes
				ShipNodeTypes.Shield => new Color(0.5f, 0.5f, 1, 0.5f), // Light Blue for shield nodes
				ShipNodeTypes.Power => new Color(1, 1, 0, 0.5f), // Light Yellow for power nodes
				_ => new Color(1, 1, 1, 0.5f) // Default color for other types
			};
		}
		else
		{
			mirrorNodeX.Visible = false;
		}

		if (MirrorY && MirrorX)
		{
			var startY = shipStartNode.GlobalPosition.Y;
			var shadow = shadowNode.GlobalPosition;

			mirrorNodeY.GlobalPosition = new Vector2(
				shadow.X,
				2 * startY - shadow.Y
			);
			
			mirrorNodeY.Modulate = currentNodeType switch
			{
				ShipNodeTypes.Weapon => new Color(1, 0.5f, 0.5f, 0.5f), // Light Red for weapon nodes
				ShipNodeTypes.Utility => new Color(0.5f, 1, 0.5f, 0.5f), // Light Green for utility nodes
				ShipNodeTypes.Shield => new Color(0.5f, 0.5f, 1, 0.5f), // Light Blue for shield nodes
				ShipNodeTypes.Power => new Color(1, 1, 0, 0.5f), // Light Yellow for power nodes
				_ => new Color(1, 1, 1, 0.5f) // Default color for other types
			};
			
			
			mirrorNodeX.Visible = true;
			
			var startX = shipStartNode.GlobalPosition.X;
			shadow = shadowNode.GlobalPosition;
			
			mirrorNodeX.GlobalPosition = new Vector2(
				2 * startX - shadow.X,
				shadow.Y
			);
			
			mirrorNodeX.Modulate = currentNodeType switch
			{
				ShipNodeTypes.Weapon => new Color(1, 0.5f, 0.5f, 0.5f), // Light Red for weapon nodes
				ShipNodeTypes.Utility => new Color(0.5f, 1, 0.5f, 0.5f), // Light Green for utility nodes
				ShipNodeTypes.Shield => new Color(0.5f, 0.5f, 1, 0.5f), // Light Blue for shield nodes
				ShipNodeTypes.Power => new Color(1, 1, 0, 0.5f), // Light Yellow for power nodes
				_ => new Color(1, 1, 1, 0.5f) // Default color for other types
			};
			
			
			mirrorNodeXY.Visible = true;
			mirrorNodeXY.GlobalPosition = new Vector2(2 * startX - shadow.X, 2 * startY - shadow.Y);
			
			mirrorNodeXY.Modulate = currentNodeType switch
			{
				ShipNodeTypes.Weapon => new Color(1, 0.5f, 0.5f, 0.5f), // Light Red for weapon nodes
				ShipNodeTypes.Utility => new Color(0.5f, 1, 0.5f, 0.5f), // Light Green for utility nodes
				ShipNodeTypes.Shield => new Color(0.5f, 0.5f, 1, 0.5f), // Light Blue for shield nodes
				ShipNodeTypes.Power => new Color(1, 1, 0, 0.5f), // Light Yellow for power nodes
				_ => new Color(1, 1, 1, 0.5f) // Default color for other types
			};
		}
		else
		{
			mirrorNodeXY.Visible = false;
		}
	}
	private void SetResourceCount()
	{		
		ResourceCount =
		[
			shipNodes.Count(n => n.NodeType == ShipNodeTypes.Weapon),
			shipNodes.Count(n => n.NodeType == ShipNodeTypes.Shield),
			shipNodes.Count(n => n.NodeType == ShipNodeTypes.Utility),
			shipNodes.Count(n => n.NodeType == ShipNodeTypes.Power)
		];
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

	public override void _UnhandledInput(InputEvent @event)
	{
		if (uiElement) return;
		if(blocked) return;
		if (@event is InputEventMouseButton mouseButtonEvent)
		{
			if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
			{
				if (PlaceStart)
				{
					PlaceStart = false;
					placing = true;
					blocked = true;
					BlockInput.Start();
				}
				else
				{
					PlaceNode(mouseButtonEvent.IsPressed());
				}

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
			if (keyEvent.Keycode == Key.Escape)
			{
				if (currentLine != null)
				{
					currentLine.StartNode.Modulate = currentLine.StartNode.NodeColor; // Reset color of the start node
					lines.Remove(currentLine); // Remove the current line from the list
					currentLine.QueueFree();
					currentLine = null; // Reset current line on Escape key
				}
				if (colorPicker != null)
				{
					colorPicker.QueueFree(); // Remove color picker if it exists
					colorPicker = null; // Reset color picker reference
				}
				dragging = false; // Reset dragging state on Escape key
				placing = true;
			}
		}
	}
	private void PlaceNode(bool pressed)
	{
		if (uiElement) return;
		Node2D clickedObject = (Node2D)DetectClickedObject()?.GetParent();
		// if (clickedObject is Line2D) return;
		if (clickedObject is ShipNode || dragging )
		{
			if (pressed)
			{
				isMouseDown = true;
				mouseDownTime = 0f;
			}
			else
			{
				isMouseDown = false;
				dragging = false;
			}

			if(!isMouseDown)
			{
				if (draggingNode != null)
				{
					CheckNodeLocation(draggingNode);
					draggingNode = null; // Stop dragging if the mouse button is released
				}
				dragging = false; // Stop dragging if the mouse button is released
			}
		}
		else if (clickedObject != null && clickedObject is not ShipLine)
		{
			PickColor();
			placing = false;
		}
		else if (pressed && placing)
		{
			bool nodeExists = shipNodes.Any(node => node.GlobalPosition == shadowNode.GlobalPosition);
			if (nodeExists)
			{
				return; // Don't place a new node if one already exists at the position
			}
			ShipNode shipNode = new ShipNode(currentNodeType);
			shipNode.GlobalPosition = shadowNode.GlobalPosition; // Snap to grid of 10 pixels
			shipNode.Name = "ShipNode_" + shipNodes.Count;
			shipNodes.Add(shipNode);
			NodesParent.AddChild(shipNode);
			
			
			if (MirrorY)
			{
				nodeExists = shipNodes.Any(node => node.GlobalPosition == mirrorNodeY.GlobalPosition);
				if (!nodeExists)
				{
					ShipNode mirroredNode = new ShipNode(currentNodeType);
					mirroredNode.GlobalPosition = mirrorNodeY.GlobalPosition;
					mirroredNode.Name = "ShipNode_" + shipNodes.Count;
					shipNodes.Add(mirroredNode);
					NodesParent.AddChild(mirroredNode);
				}
			}
			if (MirrorX)
			{
				nodeExists = shipNodes.Any(node => node.GlobalPosition == mirrorNodeX.GlobalPosition);
				if (!nodeExists)
				{
					ShipNode mirroredNode = new ShipNode(currentNodeType);
					mirroredNode.GlobalPosition = mirrorNodeX.GlobalPosition;
					mirroredNode.Name = "ShipNode_" + shipNodes.Count;
					shipNodes.Add(mirroredNode);
					NodesParent.AddChild(mirroredNode);
				}
			}

			if (MirrorX && MirrorY)
			{
				nodeExists = shipNodes.Any(node => node.GlobalPosition == mirrorNodeXY.GlobalPosition);
				if (!nodeExists)
				{
					ShipNode mirroredNode = new ShipNode(currentNodeType);
					mirroredNode.GlobalPosition = mirrorNodeXY.GlobalPosition;
					mirroredNode.Name = "ShipNode_" + shipNodes.Count;
					shipNodes.Add(mirroredNode);
					NodesParent.AddChild(mirroredNode);
				}
			}
			
			createdLast = true;
			// this is drag triangle creation
			if (clickedObject is ShipLine)
			{
				ShipLine shipLine = lines.Find(line => line == clickedObject);
				ShipLine newLine = new ShipLine(shipNode, this);
				newLine.SetEndNode(shipLine.StartNode);
				lines.Add(newLine);
				LinesParent.AddChild(newLine);

				newLine = new ShipLine(shipNode, this);
				newLine.SetEndNode(shipLine.EndNode);
				lines.Add(newLine);
				LinesParent.AddChild(newLine);
				
				TriangleCheck(newLine); // Check for triangles

				draggingNode = shipNode; // Set the newly created node as the dragging node
				dragging = true; // Start dragging the newly created node
			}
		}

		if (pressed) return;
		if (dragging) return;
		isMouseDown = false;
		dragging = false;
		draggingNode = null; // Stop dragging if the mouse button is released
		if (!isMouseDown && !createdLast && mouseDownTime < dragStartDelay)
		{
			MakeLines();
		}
		mouseDownTime = 0f;
		isPressed = false;
		createdLast = false;
	}
	public void CheckNodeLocation(ShipNode node)
	{
		ShipNode overlappingNode = shipNodes.FirstOrDefault(n => n != node && n.GlobalPosition == node.GlobalPosition);

		if (overlappingNode != null)
		{

			// Update all lines that connect to the node being removed
			foreach (var line in lines.Where(l => l.StartNode == node || l.EndNode == node).ToList())
			{
				// Check if we already have a line between overlappingNode and the other node
				ShipNode otherNode = line.StartNode == node ? line.EndNode : line.StartNode;

				// Skip if trying to create a line from a node to itself
				if (otherNode == overlappingNode)
				{
					lines.Remove(line);
					line.QueueFree();
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
					line.Points = points;
					line.UpdateCollisionShape();
				}
				else
				{
					// Remove duplicate line
					lines.Remove(line);
					line.QueueFree();
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
					triangle.QueueFree();
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
		}
	}
	private void DragNode()
	{
		if (draggingNode != null)
		{
			Vector2 newPosition = new Vector2(
				Math.Clamp(Mathf.Round(GetGlobalMousePosition().X / GridSize) * GridSize, 0, 2000),
				Math.Clamp(Mathf.Round(GetGlobalMousePosition().Y / GridSize) * GridSize, 0, 2000)
			); // Snap to grid of 10 pixels
			try
			{
				draggingNode.GlobalPosition = newPosition;
			}
			catch (Exception e)
			{
				draggingNode = null;
				return;
			}
			

			lines.ForEach(line =>
			{
				if (line.StartNode == draggingNode || line.EndNode == draggingNode)
				{
					// Create a new points array and reassign it
					var points = new Vector2[2];
					points[0] = line.StartNode.GlobalPosition;
					points[1] = line.EndNode.GlobalPosition;
					line.Points = points;
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
						currentLine.StartNode.Modulate = currentLine.StartNode.NodeColor; // Reset color of the start node
						currentLine = null; // Reset current line
						return;
					}
				});
				if (clickedShipNode == currentLine.StartNode)
				{
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

			if (MirrorY)
			{
				bool mirrrorYLine = shipNodes.Any(x => x.GlobalPosition == mirrorNodeY.GlobalPosition);
				
			}
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
		Node2D ReturnObject = null;
		foreach (var result in shapeResults)
		{

			GodotObject collider = result["collider"].As<GodotObject>();

			if (collider is Node2D hitObject)
			{
				var obj = (Node2D)hitObject.GetParent();
				if (obj is ShipNode)
				{
					return hitObject;
				}

				if (obj is ShipLine)
				{
					ReturnObject = hitObject;
				}

				if (obj is ShipTriangle && ReturnObject is not ShipLine && buttonIndex == 1)
				{
					ReturnObject = hitObject;
				}
			}
		}
		return ReturnObject;
	}
	private void TriangleCheck(ShipLine line)
	{
		foreach (ShipNode node in line.StartNode.ConnectedNodes)
		{
			if (line.EndNode.ConnectedNodes.Contains(node))
			{
				// We have a triangle, so we need to remove the line
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
			}
		}
	}
	private void PickColor()
	{
		Node2D clickedObject = (Node2D)DetectClickedObject()?.GetParent();
		if (clickedObject is ShipTriangle)
		{
			if (clickedObject != selectedTriangle || colorPicker == null)
			{
				colorPicker?.QueueFree(); // Remove the previous color picker if it exists
				colorPicker = new ColorPicker()
				{
					SamplerVisible = false,
					ColorModesVisible = false,
					SlidersVisible = false,
					HexVisible = false,
					PresetsVisible = false,
				};
				
				canvasLayer.AddChild(colorPicker);
				colorPicker.CustomMinimumSize = new Vector2(300, 300);
				colorPicker.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomLeft);


				
				selectedTriangle = clickedObject;
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
					line.QueueFree(); // Remove the line from the scene
				}

				// Remove all triangles containing this node
				var trianglesToRemove = triangles.Where(t => t.point1 == shipNode || t.point2 == shipNode || t.point3 == shipNode).ToList();
				foreach (var t in trianglesToRemove)
				{
					triangles.Remove(t);
					t.QueueFree(); // Remove the triangle from the scene
				}

				shipNode.QueueFree(); // Remove the node from the scene
			}
			// Handle right-click on lines
			else if (clickedObject is ShipLine clickedLine)
			{
				ShipLine shipLine = lines.Find(l => l == clickedLine);
				if (shipLine != null)
				{
					lines.Remove(shipLine);
					shipLine.QueueFree(); // Remove the line from the scene
					shipLine.StartNode.ConnectedNodes.Remove(shipLine.EndNode);
					shipLine.EndNode.ConnectedNodes.Remove(shipLine.StartNode);

					var trianglesWithLine = triangles.Where(t => t.lines.Contains(shipLine)).ToList();
					foreach (var t in trianglesWithLine)
					{
						triangles.Remove(t);
						t.QueueFree(); // Remove the triangle from the scene
					}
					currentLine = null; // Reset the current line if it was the one being edited
										// Remove the line from any triangles that contain it
				}
			}
			else if (clickedObject is ShipTriangle triangleNode && clickedObject is not ShipNode)
			{
				var triangle = triangles.Find(t => t == triangleNode);
				if (triangle != null)
				{
					foreach (var triangleLine in triangle.lines)
					{
						lines.Remove(triangleLine);
						triangleLine.QueueFree();
					}
					triangles.Remove(triangle);
					triangle.QueueFree();
				}
			}
		}
	}
	private void MoveShadowNode()
	{
		if (uiElement || !placing || !nodeVis)
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
				Math.Clamp(Mathf.Round(GetGlobalMousePosition().X / GridSize) * GridSize, 0, 2000),
				Math.Clamp(Mathf.Round(GetGlobalMousePosition().Y / GridSize) * GridSize, 0, 2000)
			); // Snap to grid of 10 pixels
		}
	}
	private void Buttons()
	{
		Weapon.ButtonDown += () =>
		{
			currentNodeType = ShipNodeTypes.Weapon;
		};
		Shield.ButtonDown += () =>
		{
			currentNodeType = ShipNodeTypes.Shield;
		};
		Utility.ButtonDown += () =>
		{
			currentNodeType = ShipNodeTypes.Utility;
		};
		Power.ButtonDown += () =>
		{
			currentNodeType = ShipNodeTypes.Power;
		};
	}

	private void VisibilityButtons()
	{
		NodeButton.Modulate = Colors.LightBlue;
		LineButton.Modulate = Colors.LightBlue;
		TriangleButton.Modulate = Colors.LightBlue;
    
		NodeButton.Pressed += () =>
		{
			nodeVis = ChangeVisibility(NodeButton, nodeVis);
		};
		LineButton.Pressed += () =>
		{
			lineVis = ChangeVisibility(LineButton, lineVis);
		};
		TriangleButton.Pressed += () =>
		{
			triangleVis = ChangeVisibility(TriangleButton, triangleVis);
		};
	}


	private bool ChangeVisibility(Button button, bool vis)
	{
		if (button.Modulate == Colors.LightBlue)
		{
			button.Modulate = Colors.White;
		}
		else
		{
			button.Modulate = Colors.LightBlue;
		}

		return !vis;
	}

	private void LoadButtons()
	{
		SaveButton.ButtonDown += () =>
		{
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
		shipSave.NodePositions = new Godot.Collections.Array<Vector3>();
		shipSave.NodeTypes = new Godot.Collections.Array<int>();
		shipSave.DefineTriangles = new Godot.Collections.Array<int>();
		shipSave.TriangleColors = new Godot.Collections.Array<Color>();

		foreach (var node in shipNodes)
		{
			shipSave.NodePositions.Add(new Vector3(node.GlobalPosition.X, node.GlobalPosition.Y, 0));
			shipSave.NodeTypes.Add((int)node.NodeType);
		}

		foreach (var triangle in triangles)
		{
			shipSave.DefineTriangles.Add(shipNodes.IndexOf(triangle.point1));
			shipSave.DefineTriangles.Add(shipNodes.IndexOf(triangle.point2));
			shipSave.DefineTriangles.Add(shipNodes.IndexOf(triangle.point3));
			shipSave.TriangleColors.Add(triangle.Modulate);
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
		ResourceSaver.Save(shipSave, $"{path}");
	}
	private void LoadShip(string path)
	{
		ShipSave shipSave = (ShipSave)ResourceLoader.Load($"{path}");
		if (shipSave == null)
		{
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
			line.QueueFree();
		}
		lines.Clear();

		foreach (var triangle in triangles)
		{
			triangle.QueueFree();
		}
		triangles.Clear();

		// Load nodes
		for (int i = 0; i < shipSave.NodePositions.Count; i++)
		{
			var node = new ShipNode((ShipNodeTypes)shipSave.NodeTypes[i]);
			node.GlobalPosition = new Vector2(shipSave.NodePositions[i].X, shipSave.NodePositions[i].Y);
			node.Name = "ShipNode_" + i;
			NodesParent.AddChild(node);
			shipNodes.Add(node);
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
			triangle.Modulate = shipSave.TriangleColors[index];
			foreach (var line in triangle.lines)
			{
				var pickedColor = shipSave.TriangleColors[index];
				bool isDark = pickedColor is { R: < 0.2f, G: < 0.2f, B: < 0.2f };
				line.Modulate = !isDark ? pickedColor : new Color(0.5f, 0.5f, 0.5f, 1);
			}
		}
	}
	private void RebuildAllTriangles()
	{
		// Remove all existing triangles
		foreach (var triangle in triangles)
		{
			triangle.QueueFree();
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