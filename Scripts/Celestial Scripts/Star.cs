using Godot;
using System;

public partial class Star : Node2D
{
	// starMesh;
	public MeshInstance2D starMesh;

	// Constructor
	public Star()
	{
		// Initialize the star mesh
		starMesh = new MeshInstance2D();
		starMesh.Mesh = new SphereMesh();
		starMesh.Material = new ShaderMaterial();
		starMesh.Modulate = new Color(1, 1, 0); // Yellow color
		starMesh.Scale = new Vector2(50f, 50f);
		// Add the star mesh to the star node
		AddChild(starMesh);
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
