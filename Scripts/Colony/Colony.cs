using Godot;
using System;
using System.Collections.Generic;

public partial class Colony : Node2D
{
	public Planet planet;
	public Timer popTimer;
	public int population = 5;
	public int maxPopulation = 200;
	public int growthRate = 1;
	public Colony(Planet planet)
	{
		this.planet = planet;
		GD.Print("Colony created for planet: " + planet.Name);
		growthRate = Mathf.RoundToInt(1 + (planet.Properties.Habitability * 10));
		popTimer = new Timer()
		{
			WaitTime = 1.0f,
			OneShot = true,
			Autostart = true
		};
		popTimer.Timeout += OnPopTimerTimeout;
		AddChild(popTimer);
		OnPopGroth();
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		colonyIcon();
		// Set the position of the colony
		Position = new Vector2(0, 0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Position = new Vector2(0, 0);

	}
	// Called when the timer times out
	public void OnPopTimerTimeout()
	{
		if (population < maxPopulation)
		{
			population += growthRate;
			if (population > maxPopulation)
			{
				population = maxPopulation;
			}
			GD.Print("Population: " + population);
			float populationRatio = (float)population / maxPopulation;
			planet.colonyUI.UpdateText("Population:", population.ToString());
			OnPopGroth();
			// Adjust wait time based on population ratio
			// More population = longer wait times (slower growth)
			// Less population = shorter wait times (faster growth)
			float minWaitTime = 30.0f;
			float maxWaitTime = 90.0f;
			float waitTime = minWaitTime + (populationRatio * (maxWaitTime - minWaitTime));

			popTimer.WaitTime = waitTime;
			GD.Print("Next groth in: " + waitTime);
			popTimer.Start();
		}
		else
		{
			GD.Print("Max population reached: " + maxPopulation);
		}
	}
	public void colonyIcon()
	{
		// Create a new Sprite node
		MeshInstance2D colonyIcon = new MeshInstance2D();
		colonyIcon.Name = "ColonyIcon";
		colonyIcon.Mesh = new SphereMesh();
		planet.AddChild(colonyIcon);
		colonyIcon.Position = new Vector2(planet.Properties.Radius, -(planet.Properties.Radius));
		colonyIcon.Modulate = Colors.Green;
		colonyIcon.Scale = new Vector2(4f, 4f);
		colonyIcon.ZIndex = 1; // Set the Z index to ensure it's on top of other elements

	}
	public void OnPopGroth()
	{
		// Get land positions from planet mesh
		var planetMesh = (LowPolyPlanetMesh)planet.Mesh;
		List<Vector2> landPositions = planetMesh.GetVisibleLandTrianglePositions(planet.Properties.Radius);

		// Check if we have any land positions available
		if (landPositions == null || landPositions.Count == 0)
		{
			GD.Print("No available land positions for buildings");
			return; // Exit the method if we have no positions
		}

		// Instantiate buildings at random land positions
		var random = new Random();

		int randomIndex = random.Next(landPositions.Count);
		Vector2 buildingPosition = landPositions[randomIndex];

		MeshInstance2D buildingMesh = new MeshInstance2D();
		buildingMesh.Mesh = new BoxMesh();
		buildingMesh.Scale = new Vector2(0.5f, 0.5f);
		buildingMesh.Position = buildingPosition;
		buildingMesh.Rotation = (float)GD.RandRange(0, Mathf.Pi * 2); // Random rotation
		buildingMesh.UseParentMaterial = false;
		buildingMesh.Modulate = Colors.White;
		planet.rotatingNode.AddChild(buildingMesh);

		// Remove used position to avoid duplicates
		landPositions.RemoveAt(randomIndex);
	}
}