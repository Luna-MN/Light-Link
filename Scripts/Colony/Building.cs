using Godot;
using System;

public partial class Building : Node2D
{
	public MeshInstance2D Mesh;
	public bool placed = false;
	public Color color = new Color(1, 1, 1);
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		BuildingPlacement();
	}
	public void BuildingPlacement()
	{
		if (!placed)
		{
			Modulate = new Color(color.R, color.G, color.B, 0.5f);
			GlobalPosition = GetGlobalMousePosition();
		}
		else
		{
			Modulate = new Color(color.R, color.G, color.B, 1f);
		}
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButtonEvent)
		{
			if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.Pressed)
			{
				placed = true;
			}
		}
	}

}
