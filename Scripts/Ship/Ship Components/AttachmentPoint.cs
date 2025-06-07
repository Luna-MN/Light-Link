using Godot;
using System;

public partial class AttachmentPoint : Node2D
{
	public Ship ship;
	// Called when the node enters the scene tree for the first time.
	public AttachmentPoint(Ship ship)
	{
		this.ship = ship;
		ship.AddChild(this);
		ship.attachmentPoints.Add(this);
	}
	public override void _Ready()
	{
		RemoveFromGroup("Components");
		AddToGroup("AttachmentPoints");

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
