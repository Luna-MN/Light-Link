using Godot;
using System;

public partial class Star : Node2D
{
	// starMesh;
	public MeshInstance2D Mesh;
	// Properties of the star
	public StarProperties Properties;
	// Constructor
	public Star(Vector2 pos, StarProperties properties)
	{
		// Set the position of the star
		Position = pos;
		Properties = properties;
		// Initialize the star mesh
		Mesh = new MeshInstance2D();
		Mesh.Mesh = new SphereMesh();
		Mesh.Material = new ShaderMaterial();
		Mesh.Modulate = Properties.ColorIndex; // Yellow color
		Mesh.Scale = new Vector2(Properties.Radius, Properties.Radius);
		// Add the star mesh to the star node
		AddChild(Mesh);
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
