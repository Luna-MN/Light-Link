using Godot;
using System;

public partial class AttachmentPoint : Node2D
{
	public Ship ship;
	public enum ShipNodeTypes
	{
		Weapon,
		Shield,
		Utility,
		Power,
	}
	public ShipNodeTypes NodeType;
	// Called when the node enters the scene tree for the first time.
	public AttachmentPoint()
	{
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
