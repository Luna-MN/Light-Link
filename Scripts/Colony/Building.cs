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
	private Planet previewPlanet = null; // Planet for orbit preview during placement
	private float orbitRadius = 0f; // Radius of the orbit
	[Export] private Color orbitPreviewColor = new Color(1.0f, 1f, 1f, 0.4f); // Brighter, more visible color

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		if (placed && orbitingPlanet != null)
		{
			// Update orbit angle
			orbitAngle += orbitSpeed * (float)delta;

			// Calculate new position along the orbit
			Vector2 orbitOffset = new Vector2(Mathf.Cos(orbitAngle), Mathf.Sin(orbitAngle)) * orbitRadius;
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

			// Find closest planet for preview
			previewPlanet = FindClosestPlanet();

			Vector2 orbitOffset = new Vector2(Mathf.Cos(orbitAngle), Mathf.Sin(orbitAngle)) * orbitRadius;
			Rotation = orbitOffset.Normalized().Angle() + Mathf.Pi / 2;

			// Force redraw to update the preview circle
			QueueRedraw();
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

					// Store the orbit radius
					orbitRadius = GlobalPosition.DistanceTo(orbitingPlanet.GlobalPosition);
				}
			}
		}
	}

	public override void _Draw()
	{
		if (!placed && previewPlanet != null)
		{
			Vector2 localPlanetPos = ToLocal(previewPlanet.GlobalPosition);

			// Calculate preview radius using local coordinates
			float previewRadius = localPlanetPos.Length();

			DrawArc(localPlanetPos, previewRadius, 0, Mathf.Pi * 2, 128, orbitPreviewColor, 1.0f);

			GD.Print("Drawing circle at: " + localPlanetPos + " with radius: " + previewRadius);
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButtonEvent)
		{
			if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.Pressed)
			{
				placed = true;
				previewPlanet = null; // Clear preview planet
				QueueRedraw(); // Clear the preview circle

				// Set the actual orbit radius and angle when placed
				orbitingPlanet = FindClosestPlanet();
				if (orbitingPlanet != null)
				{
					Vector2 direction = GlobalPosition - orbitingPlanet.GlobalPosition;
					orbitRadius = direction.Length();
					orbitAngle = Mathf.Atan2(direction.Y, direction.X);
				}
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

		// Debug output
		GD.Print("Found planet: " + (closestPlanet != null ? "Yes" : "No"));
		return closestPlanet;
	}
}
