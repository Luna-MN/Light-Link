using Godot;
using System;
[GlobalClass]
public partial class ShipBuilderCamera : Camera2D
{
	private bool isDragging;

	private Vector2 dragOrigin;

	[Export] public float PanSpeed = 1.0f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.Right)
			{
				if (mouseButton.Pressed)
				{
					isDragging = true;
					// Set the initial drag position when dragging starts
					dragOrigin = mouseButton.Position;

				}
				else
				{
					isDragging = false;
				}
			}
		}
		else if (@event is InputEventMouseMotion mouseMotion)
		{
			Dragging(mouseMotion);
		}
	}

	public void Dragging(InputEventMouseMotion mouseMotion)
	{
		if (isDragging)
		{
			// Calculate movement based on mouse motion and zoom level
			Vector2 movement = (dragOrigin - mouseMotion.Position) * PanSpeed / Zoom;
			Position += movement;
			dragOrigin = mouseMotion.Position;
		}
	}
}
