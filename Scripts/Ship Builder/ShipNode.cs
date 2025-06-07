using Godot;
using System;

public partial class ShipNode : Node2D
{
	public MeshInstance2D shipMesh;
	public Area2D area;
	public CollisionShape2D collisionShape;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("ShipNodes");
		shipMesh = new MeshInstance2D();
		shipMesh.Mesh = new SphereMesh(); // Example mesh, replace with your ship mesh
		shipMesh.Scale = new Vector2(10f, 10f); // Scale the mesh to a reasonable size
		AddChild(shipMesh);
		GlobalPosition = GetGlobalMousePosition(); // Set initial position to mouse position

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
