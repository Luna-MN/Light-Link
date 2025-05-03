using Godot;
using System;

public partial class Body : Node2D
{
	public LowPolyPlanetMesh Mesh;
	public Body()
	{
		CreateMesh();
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void CreateMesh()
	{
		// Initialize the planet mesh
		Mesh = new LowPolyPlanetMesh();
		// Add the planet mesh to the planet node
		AddChild(Mesh);
	}
}
