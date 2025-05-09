using Godot;
using System;
using System.Collections.Generic;

public partial class Planet : Body
{
	private TrailEffect trail;
	public PlanetProperties Properties;
	public PlanetUI planetUI;
	public Node2D rotatingNode;
	public bool hasColony = false;
	public ColonyUI colonyUI;
	public Planet(PlanetProperties properties, MeshType type = MeshType.Planet) : base(type)
	{
		Properties = properties;
		Properties.SetHabitability();
		// Set the position of the planet
		float randomAngle = (float)GD.RandRange(0, Mathf.Tau); // Random angle between 0 and 2π radians
		Position = new Vector2(
			Properties.OrbitRadius * Mathf.Cos(randomAngle),
			Properties.OrbitRadius * Mathf.Sin(randomAngle)
		);
		// Set the size of the planet
		Mesh.Scale = new Vector2(Properties.Radius, Properties.Radius);
		rotatingNode = new Node2D();
		AddChild(rotatingNode);
		rotatingNode.ZIndex = 50;

		// Set the color of the planet
		trail = new TrailEffect();
		trail.TrailLength = 500;
		SetPlanetColor();
		AddChild(trail);
		trail.SetTrailWidth(Properties.Radius * 0.7f);
		((LowPolyPlanetMesh)Mesh).ApplyPlanetProperties(Properties);
		if (Properties.HasAtmosphere)
		{
			CreateAtmosphere();
		}
		if (Properties.HasWater)
		{
		}
		GenerateMoons();
		planetUI = new PlanetUI();
		AddChild(planetUI);
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		planetUI.SetPlanetProperties(Properties);
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Rotate the planet around the star
		Orbit(Properties.OrbitPeriod, (float)delta);
		Rotate(Properties.RotationPeriod, (float)delta);
	}
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
		else
		{
			// For non-water planets, avoid blue hues (0.55-0.7)
			if (hue >= 0.55f && hue <= 0.7f)
			{
				// Shift blue colors to green or purple
				if (GD.Randf() < 0.5f)
					hue = (float)GD.RandRange(0.25f, 0.4f); // Shift to green
				else
					hue = (float)GD.RandRange(0.75f, 0.9f); // Shift to purple/magenta
			}
		}

		// Atmosphere makes the planet slightly lighter and less saturated
		if (Properties.HasAtmosphere)
		{
			value = Mathf.Clamp(value + 0.1f, 0f, 1f);
			saturation = Mathf.Clamp(saturation - 0.1f, 0f, 1f);
		}

		// Create color from HSV values
		Color planetColor = Color.FromHsv(hue, saturation, value);
		Properties.ColorIndex = planetColor;
		// Make trail color slightly darker than the planet
		Color trailColor = Properties.ColorIndex.Darkened(0.2f);
		trailColor.A = 0.7f;
		trail.SetTrailColor(trailColor);
		// Apply the color to the planet mesh
		Mesh.Modulate = planetColor;
	}
	public void CreateAtmosphere()
	{
		// Create an atmosphere mesh
		MeshInstance2D atmosphereMesh = new MeshInstance2D();
		atmosphereMesh.Mesh = new SphereMesh();
		atmosphereMesh.Material = new ShaderMaterial();
		atmosphereMesh.Scale = new Vector2(Properties.Radius * 1.1f, Properties.Radius * 1.1f);
		Color atmosphereColor = Properties.ColorIndex; // Set the color of the atmosphere
		atmosphereColor.A = 0.2f;
		atmosphereColor = atmosphereColor.Lightened(0.7f);
		atmosphereMesh.Modulate = atmosphereColor; // Planet color with transparency
		AddChild(atmosphereMesh);
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
	#region Planet Rotation Functions
	public void Rotate(float speed, float time)
	{
		// Rotate the planet around its own axis
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
		Mesh.Rotation += angle;
		rotatingNode.Rotation += angle;
	}
	#endregion
	#region Moon Functions
	public void GenerateMoons()
	{
		// Generate moons around the planet
		for (int i = 0; i < Properties.Moons; i++)
		{
			float orbitRadius = ((i + 1) * 10f) + Properties.Radius; // in AU
			float mass = new RandomNumberGenerator().RandfRange(0.1f, 10f); // Random mass between 0.1 and 10 Earth masses
			MoonProperties moonProperties = new MoonProperties
			{
				OrbitRadius = orbitRadius,
				Mass = mass,
				Radius = 1 + (mass * 0.15f), // Radius scales with mass
				OrbitPeriod = new RandomNumberGenerator().RandfRange(0.1f, 1f) * 20,
				HasWater = mass > 6.5f,
			};
			// Create a moon instance
			Moon moon = new Moon(moonProperties);
			AddChild(moon);
		}
	}
	#endregion
}
