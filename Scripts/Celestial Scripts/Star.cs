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
		CreateStarMesh();
		GeneratePlanets();
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	#region Star Mesh Functions
	public void CreateStarMesh()
	{
		// Initialize the star mesh
		Mesh = new MeshInstance2D();
		Mesh.Mesh = new SphereMesh();
		Mesh.Material = new ShaderMaterial();
		Mesh.Modulate = Properties.ColorIndex; // Yellow color
		Mesh.Scale = new Vector2(Properties.Radius, Properties.Radius);
		// Add the star mesh to the star node
		AddChild(Mesh);
	}
	#endregion
	#region Planet Functions
	public void GeneratePlanets()
	{
		// Generate planets around the star
		for (int i = 0; i < Properties.Planets; i++)
		{
			// Create a new planet properties instance
			PlanetProperties planetProperties = new PlanetProperties
			{
				Mass = 1, // in Earth Masses
				Radius = 30, // in Earth Radii
				OrbitRadius = (i + 1) * 100f, // in AU (Astronomical Units)
				OrbitPeriod = new RandomNumberGenerator().Randf(), // in Earth days
				RotationPeriod = 24, // in Earth hours
				AxialTilt = 23.5f, // in degrees
				ColorIndex =
					new Color(
						0.5f + (float)i / Properties.Planets,
						0.5f + (float)i / Properties.Planets,
						1 - (float)i / Properties.Planets
					),
				HasAtmosphere = true,
				HasWater = true
			};
			// Create a new planet
			Planet planet = new Planet(planetProperties);
			// Add the planet to the star
			AddChild(planet);
		}
	}
	#endregion
}
