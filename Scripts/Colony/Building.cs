using Godot;
using System;

public partial class Building : Node2D
{
	public MeshInstance2D Mesh;
	public bool placed = false;
	public Color color = new Color(1, 1, 1);

	private float orbitAngle = 0f; // Angle for orbiting
	private float orbitSpeed = 0.25f; // Speed of orbiting
	private Planet orbitingPlanet = null; // The planet the building is orbiting

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (placed && orbitingPlanet != null)
		{
			// Update orbit angle
			orbitAngle += orbitSpeed * (float)delta;

			// Calculate new position along the orbit
			Vector2 orbitOffset = new Vector2(Mathf.Cos(orbitAngle), Mathf.Sin(orbitAngle)) * 100f; // Adjust radius as needed
			GlobalPosition = orbitingPlanet.GlobalPosition + orbitOffset;

			// Rotate building to face tangent to the orbit
			Rotation = orbitOffset.Normalized().Angle() + Mathf.Pi / 2;
		}
		else
		{
			BuildingPlacement();
		}
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
			if (orbitingPlanet == null)
			{
				orbitingPlanet = FindClosestPlanet();
				if (orbitingPlanet != null)
				{
					// Set initial orbit angle based on current position
					Vector2 direction = GlobalPosition - orbitingPlanet.GlobalPosition;
					orbitAngle = Mathf.Atan2(direction.Y, direction.X);
				}
			}
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

	public Planet FindClosestPlanet()
	{
		Planet closestPlanet = null;
		float closestDistance = float.MaxValue;
		foreach (var planet in GetTree().GetNodesInGroup("planets"))
		{
			if (planet is Planet p)
			{
				float distance = GlobalPosition.DistanceTo(p.GlobalPosition);
				if (distance < closestDistance)
				{
					closestDistance = distance;
					closestPlanet = p;
				}
			}
		}
		return closestPlanet;
	}
}
