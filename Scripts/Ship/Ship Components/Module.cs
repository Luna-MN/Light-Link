using Godot;
using System;

public partial class Module : Node2D
{
	public bool placed;
	public Node2D meshParent;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Modulate = new Color(1, 1, 1, 0.5f);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!placed)
		{
			Position = GetGlobalMousePosition();
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButtonEvent)
		{
			if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.Pressed)
			{
				placed = true;
				Modulate = new Color(1, 1, 1, 1);
			}
		}
	}
}
	