using Godot;
using System;

public partial class Moon : Body
{
	MoonProperties Properties;
	// Constructor
	public Moon(MoonProperties properties) : base(MeshType.Moon)
	{
		Properties = properties;
		Mesh.Radius = properties.Radius;
		Position = new Vector2(Properties.OrbitRadius, 0);

		MoonColor();
		if (Mesh is LowPolyMoonMesh moonMesh)
		{
			moonMesh.ConfigureFromProperties(Properties);
		}
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Rotate the moon around the planet
		Orbit(Properties.OrbitPeriod, (float)delta);
	}
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
	public void MoonColor()
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
			hue = Mathf.Clamp(hue + 0.2f, 0f, 1f);
			saturation = Mathf.Clamp(saturation - 0.2f, 0f, 1f);
			value = Mathf.Clamp(value + 0.2f, 0f, 1f);
		}
		else
		{
			hue = Mathf.Clamp(hue - 0.2f, 0f, 1f);
			saturation = Mathf.Clamp(saturation + 0.2f, 0f, 1f);
			value = Mathf.Clamp(value - 0.2f, 0f, 1f);
		}
		Properties.ColorIndex = new Color(hue, saturation, value);
	}
}
