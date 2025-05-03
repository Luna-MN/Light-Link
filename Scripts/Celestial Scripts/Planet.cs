using Godot;
using System;
using System.Collections.Generic;

public partial class Planet : Body
{
	private TrailEffect trail;
	public PlanetProperties Properties;
	public Planet(PlanetProperties properties)
	{
		Properties = properties;
		// Set the position of the planet
		Position = new Vector2(Properties.OrbitRadius, 0);
		// Set the size of the planet
		Mesh.Scale = new Vector2(Properties.Radius, Properties.Radius);

		// Set the color of the planet
		trail = new TrailEffect();
		trail.TrailLength = 300;
		SetPlanetColor();

		AddChild(trail);
		trail.SetTrailWidth(Properties.Radius * 0.5f);
		if (Properties.HasAtmosphere)
		{
			CreateAtmosphere();
		}
		if (Properties.HasWater)
		{
		}
		Mesh.ApplyPlanetProperties(Properties);
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
		Properties.ColorIndex = planetColor;
		// Make trail color slightly darker than the planet
		Color trailColor = planetColor.Darkened(0.2f);
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
		Color atmosphereColor = Mesh.Modulate; // Set the color of the atmosphere
		atmosphereColor.A = 0.3f;
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
}
