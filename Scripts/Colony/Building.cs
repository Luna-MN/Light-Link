using Godot;
using System;

public partial class Building : Node2D
{
	public MeshInstance2D Mesh;
	public bool placed = false;
	public Color color = new Color(1, 1, 1);

	private float orbitAngle = 0f; // Angle for orbiting
	private float orbitSpeed = 0.05f; // Speed of orbiting
	private Body orbitingBody = null; // The planet the building is orbiting
	private Body previewBody = null; // Planet for orbit preview during placement
	public float orbitRadius = 0f; // Radius of the orbit
	[Export] private Color orbitPreviewColor = new Color(1.0f, 1f, 1f, 0.4f); // Brighter, more visible color

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		if (placed && orbitingBody != null)
		{
			// Update orbit angle
			orbitAngle += orbitSpeed * (float)delta;

			// Calculate new position along the orbit
			Vector2 orbitOffset = new Vector2(Mathf.Cos(orbitAngle), Mathf.Sin(orbitAngle)) * orbitRadius;
			GlobalPosition = orbitingBody.GlobalPosition + orbitOffset;

			// Rotate building to face tangent to the orbit
			Rotation = orbitOffset.Normalized().Angle() + Mathf.Pi / 2;
		}
		else
		{
			if (this is ResourcePlace)
			{
				BuildingPlacement(true); // For ResourcePlace, allow orbiting around the sun
			}
			else
			{
				BuildingPlacement();
			}

		}
	}

	public void BuildingPlacement(bool SunOrbit = false)
	{
		if (!placed)
		{
			Modulate = new Color(color.R, color.G, color.B, 0.5f);
			GlobalPosition = GetGlobalMousePosition();

			// Find closest planet for preview
			if (SunOrbit)
			{
				previewBody = FindClosestStar();
			}
			else
			{
				previewBody = FindClosestPlanet();
			}

			Vector2 orbitOffset = new Vector2(Mathf.Cos(orbitAngle), Mathf.Sin(orbitAngle)) * orbitRadius;
			Rotation = orbitOffset.Normalized().Angle() + Mathf.Pi / 2;

			// Force redraw to update the preview circle
			QueueRedraw();
		}
		else
		{
			Modulate = new Color(color.R, color.G, color.B, 1f);
			if (orbitingBody == null)
			{
				if (SunOrbit)
				{
					orbitingBody = FindClosestStar();
				}
				else
				{
					// Find the closest planet to orbit around
					orbitingBody = FindClosestPlanet();
				}
				if (orbitingBody != null)
				{
					// Set initial orbit angle based on current position
					Vector2 direction = GlobalPosition - orbitingBody.GlobalPosition;
					orbitAngle = Mathf.Atan2(direction.Y, direction.X);

					// Store the orbit radius
					if (orbitRadius == 0f)
					{
						orbitRadius = GlobalPosition.DistanceTo(orbitingBody.GlobalPosition);
					}
				}
			}
		}
	}

	public override void _Draw()
	{
		if (!placed && previewBody != null)
		{
			Vector2 localPlanetPos = ToLocal(previewBody.GlobalPosition);

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
				previewBody = null; // Clear preview planet
				QueueRedraw(); // Clear the preview circle

				// Set the actual orbit radius and angle when placed
				if (this is ResourcePlace)
				{
					orbitingBody = FindClosestStar();
				}
				else
				{
					// Find the closest planet to orbit around
					orbitingBody = FindClosestPlanet();
				}
				if (orbitingBody != null)
				{
					Vector2 direction = GlobalPosition - orbitingBody.GlobalPosition;
					if (orbitRadius == 0f)
					{
						// Set the orbit radius based on the distance to the planet
						orbitRadius = direction.Length();
					}
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
	public Star FindClosestStar()
	{
		Star closestStar = null;
		float closestDistance = float.MaxValue;
		foreach (var star in GetTree().GetNodesInGroup("Stars"))
		{
			if (star is Star s)
			{
				float distance = GlobalPosition.DistanceTo(s.GlobalPosition);
				if (distance < closestDistance)
				{
					closestDistance = distance;
					closestStar = s;
				}
			}
		}

		return closestStar;
	}
}
