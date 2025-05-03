using Godot;
using System;

public partial class Planet : Node2D
{
	public MeshInstance2D Mesh;
	public PlanetProperties Properties;
	public Planet(PlanetProperties properties)
	{
		this.Properties = properties;
		// Set the position of the planet
		Position = new Vector2(Properties.OrbitRadius, 0);
		// Initialize the planet mesh
		CreatePlanetMesh();
		// Set the speed of the planet

	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Rotate the planet around the star
		Orbit(Properties.OrbitPeriod, (float)delta);
	}
	#region Planet Mesh Functions
	public void CreatePlanetMesh()
	{
		// Initialize the planet mesh
		Mesh = new MeshInstance2D();
		Mesh.Mesh = new SphereMesh();
		Mesh.Material = new ShaderMaterial();
		Mesh.Modulate = Properties.ColorIndex; // Set the color of the planet
		Mesh.Scale = new Vector2(Properties.Radius, Properties.Radius); // Set the size of the planet

		// Add the planet mesh to the planet node
		AddChild(Mesh);
	}

	#endregion
	#region Planet Orbit Functions
	public void Orbit(float speed, float time)
	{
		// Rotate the planet around the star
		float angle = speed * time;
		Position = new Vector2(
			Position.X * Mathf.Cos(angle) - Position.Y * Mathf.Sin(angle),
			Position.X * Mathf.Sin(angle) + Position.Y * Mathf.Cos(angle)
		);
	}
	#endregion
}
