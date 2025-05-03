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
		// Set the color of the planet
		SetPlanetColor();

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
	#region Planet Properties Functions
	public void SetPlanetColor()
	{
		// Base color components
		float hue;
		float saturation = 0.8f;
		float value = 0.8f;

		// Orbit radius affects the base hue with full color spectrum
		// Closer planets are redder (hot), distant planets are bluer (cold)
		float normalizedRadius = Mathf.Clamp(Properties.OrbitRadius / 1000f, 0f, 1f);

		// Use full color spectrum and apply slight randomization
		float randomOffset = (float)GD.RandRange(-0.05, 0.05);
		hue = Mathf.Clamp(normalizedRadius + randomOffset, 0f, 1f);

		// Water influences - make more distinctly blue rather than green-blue
		if (Properties.HasWater)
		{
			hue = Mathf.Lerp(hue, 0.65f, 0.3f); // More definitively blue (0.65)
			saturation = Mathf.Clamp(saturation + 0.15f, 0f, 1f);
		}

		// Atmosphere makes the planet slightly lighter and less saturated
		if (Properties.HasAtmosphere)
		{
			value = Mathf.Clamp(value + 0.1f, 0f, 1f);
			saturation = Mathf.Clamp(saturation - 0.1f, 0f, 1f);
		}

		// Create color from HSV values
		Color planetColor = Color.FromHsv(hue, saturation, value);

		// Apply the color to the planet mesh
		Mesh.Modulate = planetColor;
	}
	#endregion
	#region Planet Orbit Functions
	public void Orbit(float speed, float time)
	{
		// Rotate the planet around the star
		float angle = speed * time;
		if (angle > 360)
		{
			angle = 0;
		}
		else if (angle < -360)
		{
			angle = 0;
		}
		else
		{
			angle = Mathf.DegToRad(angle);
		}
		Position = new Vector2(
			Position.X * Mathf.Cos(angle) - Position.Y * Mathf.Sin(angle),
			Position.X * Mathf.Sin(angle) + Position.Y * Mathf.Cos(angle)
		);
	}
	#endregion
}
