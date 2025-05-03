using Godot;
using System;

public partial class Main : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Create a new star instance
		Star star = new Star();

		// Set the position of the star
		star.Position = new Vector2(100, 100);

		// Add the star to the scene
		AddChild(star);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
