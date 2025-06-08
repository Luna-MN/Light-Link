using Godot;
using System;
using System.Collections.Generic;

public partial class ShipNode : Node2D
{
	public MeshInstance2D shipMesh;
	public Area2D area;
	public CollisionShape2D collisionShape;
	public List<ShipNode> connectedNodes = new List<ShipNode>();
	public ShipBuilder.ShipNodeTypes nodeType = ShipBuilder.ShipNodeTypes.Weapon;
	public Color nodeColor = Colors.White; // Default color for the node
	public ShipNode(ShipBuilder.ShipNodeTypes type)
	{
		nodeType = type;
		Name = "ShipNode";
		if (type == ShipBuilder.ShipNodeTypes.Weapon)
		{
			nodeColor = new Color(1, 0.5f, 0.5f); // Light Red for weapon nodes
		}
		else if (type == ShipBuilder.ShipNodeTypes.Utility)
		{
			nodeColor = new Color(0.5f, 1, 0.5f); // Light Green for utility nodes
		}
		else if (type == ShipBuilder.ShipNodeTypes.Shield)
		{
			nodeColor = new Color(0.5f, 0.5f, 1); // Light Blue for shield nodes
		}
		else if (type == ShipBuilder.ShipNodeTypes.Power)
		{
			nodeColor = new Color(1, 1, 0); // Light Yellow for power nodes
		}
		else
		{
			nodeColor = Colors.White; // Default color for other types
		}
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Modulate = nodeColor; // Set the node's color based on its type
		ZIndex = 1000; // Ensure the ship node is drawn above other nodes
		AddToGroup("ShipNodes");
		shipMesh = new MeshInstance2D();
		shipMesh.Mesh = new SphereMesh(); // Example mesh, replace with your ship mesh
		shipMesh.Scale = new Vector2(10f, 10f); // Scale the mesh to a reasonable size
		AddChild(shipMesh);

		area = new Area2D();


		collisionShape = new CollisionShape2D();
		collisionShape.Shape = new CircleShape2D();
		((CircleShape2D)collisionShape.Shape).Radius = 10f; // Set the radius of the collision shape

		area.AddChild(collisionShape); // Add the collision object to the area
		AddChild(area); // Add the area to the ship node

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
