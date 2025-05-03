using Godot;
using System;

public partial class Star : Body
{
	// Properties of the star
	public StarProperties Properties;
	// Constructor
	public Star(Vector2 pos, StarProperties properties)
	{
		// Set the position of the star
		Properties = properties;

		Position = pos;
		Mesh.Modulate = Properties.ColorIndex; // Yellow color
		Mesh.Scale = new Vector2(Properties.Radius, Properties.Radius);

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
	#region Planet Functions
	public void GeneratePlanets()
	{
		// Water exists in a "Goldilocks zone" - not too hot, not too cold
		// Also requires sufficient mass to retain water
		float habitableZoneInner = Properties.Radius * 6.0f;
		float habitableZoneOuter = Properties.Radius * 9.0f;

		// Generate planets around the star
		for (int i = 0; i < Properties.Planets; i++)
		{
			float orbitRadius = (i + 1) * 100f; // in AU
			float mass = new RandomNumberGenerator().RandfRange(0.1f, 10f); // Random mass between 0.1 and 10 Earth masses

			// Habitability calculations
			// Determine if planet can retain atmosphere based on mass and distance
			// Small planets and those too close to star struggle to maintain atmospheres
			bool hasAtmosphere = mass >= 4f &&
							   orbitRadius > Properties.Radius * 5f;

			bool hasWater = mass >= 0.3f &&
						   orbitRadius >= habitableZoneInner &&
						   orbitRadius <= habitableZoneOuter;

			// Create a new planet properties instance
			PlanetProperties planetProperties = new PlanetProperties
			{
				Mass = mass,
				Radius = 10 + (mass * 3), // Radius scales with mass
				OrbitRadius = orbitRadius,
				OrbitPeriod = new RandomNumberGenerator().RandfRange(0.1f, 1f) * 100,
				RotationPeriod = 24,
				HasAtmosphere = hasAtmosphere,
				HasWater = hasWater
			};
			GD.Print(hasAtmosphere, hasWater);
			// Create a new planet
			Planet planet = new Planet(planetProperties);
			// Add the planet to the star
			AddChild(planet);
		}
	}
	#endregion
}
