using Godot;
using System;

public partial class Component : Node2D
{
	public Ship ship; // Reference to the ship this component is attached to
					  // Called when the node enters the scene tree for the first time.
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
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
