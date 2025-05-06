using Godot;
using System;

public partial class Colony : Node2D
{
	public Planet planet;

	public Colony(Planet planet)
	{
		this.planet = planet;
		GD.Print("Colony created for planet: " + planet.Name);
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Set the position of the colony
		Position = new Vector2(0, 0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}