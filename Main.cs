using Godot;
using System;

public partial class Main : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Create a new star properties instance
		StarProperties starProperties = new StarProperties
		{
			Tempreture = 5300, // in Kelvin
			Luminosity = 1, // in Solar Luminosity
			Mass = 1, // in Solar Masses
			Radius = 50, // in Solar Radii
			Age = 4.6f, // in billion years
			ColorIndex = new Color(1, 1, 0), // Color index for the star
			Planets = 7 // Number of planets orbiting the star
		};
		// Create a new star instance
		Star star = new Star(new Vector2(500, 500), starProperties);


		// Add the star to the scene
		AddChild(star);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
