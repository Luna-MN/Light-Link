using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public Camera2D camera;
	public Vector2 SunIncrease = new Vector2(5, 5);
	public bool isZoomedOut = false;

	// New fields for smooth scaling
	private float scaleTransitionSpeed = 5.0f; // Adjust this value to control transition speed
	private Dictionary<Star, Vector2> targetStarScales = new Dictionary<Star, Vector2>();

	public override void _Ready()
	{
		camera = GetViewport().GetCamera2D();
		// Create a new star properties instance
		StarProperties starProperties = new StarProperties
		{
			Tempreture = 5300, // in Kelvin
			Age = 4.6f, // in billion years
		};
		// Create a new star instance
		Star star = new Star(new Vector2(500, 500), starProperties);

		// Add the star to the scene
		AddChild(star);

		// Initialize target scale for the star
		targetStarScales[star] = star.Mesh.Scale;
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

		foreach (Node2D child in GetChildren())
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
		if (camera.Zoom.X >= 0.2f)
		{
			foreach (Node2D child in GetChildren())
			{
				if (child is Star star)
				{
					foreach (Node2D planet in star.GetChildren())
					{
						if (planet is Planet planetBody)
						{
							planetBody.Visible = true;
						}
						if (planet is Node2D astroid)
						{
							foreach (Node2D a in astroid.GetChildren())
							{
								if (a is Astroid astroidBody)
								{
									astroidBody.Visible = true;
								}
							}
						}
					}

					if (isZoomedOut)
					{
						// Instead of immediately changing scale, set the target scale
						targetStarScales[star] = star.Mesh.Scale / SunIncrease;
						isZoomedOut = false;
					}
				}
			}
		}
		else if (camera.Zoom.X < 0.2f)
		{
			foreach (Node2D child in GetChildren())
			{
				if (child is Star star)
				{
					foreach (Node2D planet in star.GetChildren())
					{
						if (planet is Planet planetBody)
						{
							planetBody.Visible = false;
						}
						if (planet is Node2D astroid)
						{
							foreach (Node2D a in astroid.GetChildren())
							{
								if (a is Astroid astroidBody)
								{
									astroidBody.Visible = false;
								}
							}
						}
					}

					if (!isZoomedOut)
					{
						// Instead of immediately changing scale, set the target scale
						targetStarScales[star] = star.Mesh.Scale * SunIncrease;
						isZoomedOut = true;
					}
				}
			}
		}

		if (camera.Zoom.X >= 0.5f)
		{
			foreach (Node2D child in GetChildren())
			{
				if (child is Star star)
				{
					foreach (Node2D planet in star.GetChildren())
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
			foreach (Node2D child in GetChildren())
			{
				if (child is Star star)
				{
					foreach (Node2D planet in star.GetChildren())
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
}
