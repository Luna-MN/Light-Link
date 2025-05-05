using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Main : Node2D
{
	public UI ui;
	// Called when the node enters the scene tree for the first time.
	public Camera2D camera;
	public Vector2 SunIncrease = new Vector2(5, 5);
	public bool isZoomedOut = false;

	// New fields for smooth scaling
	private float scaleTransitionSpeed = 5.0f; // Adjust this value to control transition speed
	private Dictionary<Star, Vector2> targetStarScales = new Dictionary<Star, Vector2>();
	// Add this to store the original scales
	private Dictionary<Star, Vector2> originalStarScales = new Dictionary<Star, Vector2>();

	// In _Ready() method:
	public override void _Ready()
	{
		camera = GetViewport().GetCamera2D();
		ui = (UI)GetChildren().FirstOrDefault(x => x is UI ui);

		foreach (Node child in GetChildren())
		{
			if (child is Star star)
			{
				originalStarScales[star] = star.Mesh.Scale;
			}
		}


		CreateUniverse(20, 4000, new Vector2(20000, 20000));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		CheckZoom();
		UpdateStarScales(delta);
	}

	private void UpdateStarScales(double delta)
	{
		float lerpFactor = (float)(scaleTransitionSpeed * delta);

		foreach (Node child in GetChildren())
		{
			if (child is Star star && targetStarScales.ContainsKey(star))
			{
				// Smoothly interpolate to the target scale
				star.Mesh.Scale = new Vector2(
					Mathf.Lerp(star.Mesh.Scale.X, targetStarScales[star].X, lerpFactor),
					Mathf.Lerp(star.Mesh.Scale.Y, targetStarScales[star].Y, lerpFactor)
				);
			}
		}
	}

	public void CheckZoom()
	{
		bool shouldBeZoomedOut = camera.Zoom.X < 0.2f;

		// Only update scales if zoom state changes
		if (shouldBeZoomedOut != isZoomedOut)
		{
			foreach (Node child in GetChildren())
			{
				if (child is Star star && originalStarScales.ContainsKey(star))
				{
					if (shouldBeZoomedOut)
					{
						// Set target based on original scale when zooming out
						targetStarScales[star] = originalStarScales[star] * SunIncrease;
					}
					else
					{
						// Restore original scale when zooming in
						targetStarScales[star] = originalStarScales[star];
					}
				}
			}

			isZoomedOut = shouldBeZoomedOut;
		}

		// Handle planet visibility
		if (camera.Zoom.X >= 0.2f)
		{
			// Show planets
			foreach (Node child in GetChildren())
			{
				if (child is Star star)
				{
					foreach (Node planet in star.GetChildren())
					{
						if (planet is Planet planetBody)
						{
							planetBody.Visible = true;
						}
						if (planet is Node2D astroid)
						{
							foreach (Node a in astroid.GetChildren())
							{
								if (a is Astroid astroidBody)
								{
									astroidBody.Visible = true;
								}
							}
						}
					}
				}
			}
		}
		else
		{
			// Hide planets
			foreach (Node child in GetChildren())
			{
				if (child is Star star)
				{
					foreach (Node planet in star.GetChildren())
					{
						if (planet is Planet planetBody)
						{
							planetBody.Visible = false;
						}
						if (planet is Node2D astroid)
						{
							foreach (Node a in astroid.GetChildren())
							{
								if (a is Astroid astroidBody)
								{
									astroidBody.Visible = false;
								}
							}
						}
					}
				}
			}
		}

		// Handle moon visibility
		if (camera.Zoom.X >= 0.5f)
		{
			foreach (Node child in GetChildren())
			{
				if (child is Star star)
				{
					foreach (Node planet in star.GetChildren())
					{
						if (planet is Planet planetBody)
						{
							foreach (Node2D moon in planetBody.GetChildren())
							{
								if (moon is Moon moonBody)
								{
									moonBody.Visible = true;
								}
							}
						}
					}
				}
			}
		}
		else if (camera.Zoom.X < 0.5f)
		{
			foreach (Node child in GetChildren())
			{
				if (child is Star star)
				{
					foreach (Node planet in star.GetChildren())
					{
						if (planet is Planet planetBody)
						{
							foreach (Node2D moon in planetBody.GetChildren())
							{
								if (moon is Moon moonBody)
								{
									moonBody.Visible = false;
								}
							}
						}
					}
				}
			}
		}
	}

	public Star CreateStar(Vector2 position, StarProperties starProperties)
	{
		// Create a new star instance
		Star star = new Star(position, starProperties);

		// Add the star to the scene
		AddChild(star);

		// Store the original scale when creating the star
		originalStarScales[star] = star.Mesh.Scale;
		// Initialize target scale to the original scale
		targetStarScales[star] = star.Mesh.Scale;

		return star;
	}

	public void CreateUniverse(int starCount = 10, float minDistance = 500f, Vector2? universeSize = null)
	{
		// Default universe size if not specified
		Vector2 size = universeSize ?? new Vector2(10000, 10000);

		// List to keep track of star positions
		List<Vector2> starPositions = new List<Vector2>();

		// Create stars
		int attempts = 0;
		int maxAttempts = starCount * 10; // Safety limit to prevent infinite loops
		Random random = new Random();

		while (starPositions.Count < starCount && attempts < maxAttempts)
		{
			// Generate a random position within the universe bounds
			Vector2 newPosition = new Vector2(
				(float)random.NextDouble() * size.X,
				(float)random.NextDouble() * size.Y
			);

			// Check if the position is far enough from existing stars
			bool validPosition = true;
			foreach (Vector2 existingPosition in starPositions)
			{
				if (newPosition.DistanceTo(existingPosition) < minDistance)
				{
					validPosition = false;
					break;
				}
			}

			if (validPosition)
			{
				// Generate random star properties
				StarProperties starProperties = GenerateRandomStarProperties(random);

				// Create the star at the valid position
				Star star = CreateStar(newPosition, starProperties);
				// Set the star's name
				star.Name = $"Star_{starPositions.Count + 1}";
				// Add the position to the list
				starPositions.Add(newPosition);
				// No need to initialize targetStarScales here as it's already done in CreateStar
			}

			attempts++;
		}
	}

	private StarProperties GenerateRandomStarProperties(Random random)
	{
		// Use a weighted distribution centered around 5300K
		float targetTemp = 5300f; // Target temperature in Kelvin
		float stdDev = 1500f;     // Standard deviation for temperature distribution

		// Generate temperature using Box-Muller transform for normal distribution
		float u1 = (float)random.NextDouble();
		float u2 = (float)random.NextDouble();
		float randStdNormal = (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2));
		float temp = targetTemp + stdDev * randStdNormal;

		// Ensure temperature is within reasonable bounds (2000K to 30000K)
		temp = Math.Clamp(temp, 2000f, 30000f);

		// Age ranges for stars (in billion years)
		float[] ageRanges = { 0.1f, 0.5f, 1.0f, 2.0f, 4.0f, 5.0f, 8.0f, 10.0f };

		// Randomly select age
		float age = ageRanges[random.Next(ageRanges.Length)];

		// Add some randomness to the selected age
		age += (float)(random.NextDouble() * 0.5 - 0.25);

		// Ensure age is positive
		age = Math.Max(0.1f, age);

		return new StarProperties
		{
			Temperature = temp,
			Age = age
		};
	}
}
