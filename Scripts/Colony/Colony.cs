using Godot;
using System;

public partial class Colony : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Set the position of the colony
		Position = new Vector2(0, 0);
		// Set the scale of the colony
		Scale = new Vector2(0.1f, 0.1f);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}