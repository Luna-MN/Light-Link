using Godot;
using System;

public partial class Component : Node2D
{
	public Ship ship; // Reference to the ship this component is attached to
					  // Called when the node enters the scene tree for the first time.

	// component properties
	public int powerConsumption = 0; // Power consumption of the component
	public int powerProduction = 0; // Power production of the component
	public int maxHealth = 100; // Maximum health of the component
	public int currentHealth = 100; // Current health of the component

	public Component(AttachmentPoint attachmentPoint)
	{
		attachmentPoint.AddChild(this);
		attachmentPoint.ship.attachmentPoints.Remove(attachmentPoint);
		attachmentPoint.ship.attachedComponents.Add(this);
		ship = attachmentPoint.ship;
	}
	public override void _Ready()
	{
		AddToGroup("Components");
		ship.powerLeft += powerProduction; // Add power production to the ship's power
		ship.powerLeft -= powerConsumption; // Deduct power consumption from the ship's power
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
