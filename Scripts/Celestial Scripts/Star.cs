using Godot;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public partial class Star : Body
{
	// Properties of the star
	public StarProperties Properties;
	// Constructor
	public Star(Vector2 pos, StarProperties properties, MeshType type = MeshType.Star) : base(type)
	{
		// Set the position of the star
		Properties = properties;

		Position = pos;
		GD.Print(type);
		if (Mesh is LowPolyStarMesh starMesh)
		{
			starMesh.GenerateStar(Properties.Tempreture, 0.6f);
		}
		else
		{
			GD.Print($"Wrong mesh type: {Mesh.GetType().Name}, creating correct one");
			// Create the correct mesh type
			Mesh = new LowPolyStarMesh();
			((LowPolyStarMesh)Mesh).GenerateStar(Properties.Tempreture, 0.6f);
		}
		Mesh.Scale = new Vector2(Properties.Radius, Properties.Radius);
		// Apply star-specific visual properties
		ShaderMaterial material = Mesh.Material as ShaderMaterial;
		Properties.SetResorces();
		GeneratePlanets();

		GenerateAstroids();
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	#region Star Properties
	public void SetStarProperties()
	{
		if (Properties.Tempreture <= 3200)
		{
			Properties.SType = StarProperties.StarType.M;
		}
		else if (Properties.Tempreture <= 4000)
		{
			Properties.SType = StarProperties.StarType.K;
		}
		else if (Properties.Tempreture <= 5200)
		{
			Properties.SType = StarProperties.StarType.KG;
		}
		else if (Properties.Tempreture < 6000)
		{
			Properties.SType = StarProperties.StarType.G;
		}
		else if (Properties.Tempreture < 7200)
		{
			Properties.SType = StarProperties.StarType.F;
		}
		else if (Properties.Tempreture < 10000)
		{
			Properties.SType = StarProperties.StarType.A;
		}
		else if (Properties.Tempreture < 30000)
		{
			Properties.SType = StarProperties.StarType.B;
		}
		else
		{
			Properties.SType = StarProperties.StarType.O;
		}
	}
	#endregion
	#region Planet Functions
	public void GeneratePlanets()
	{
		// Water exists in a "Goldilocks zone" - not too hot, not too cold
		// Also requires sufficient mass to retain water
		float habitableZoneInner = Properties.Radius * 6.0f;
		float habitableZoneOuter = Properties.Radius * 9.0f;

		// Generate planets around the star
		for (int i = 0; i < Properties.Planets; i++)
		{
			float orbitRadius = (i + 1) * 100f; // in AU
			float mass = new RandomNumberGenerator().RandfRange(0.1f, 10f); // Random mass between 0.1 and 10 Earth masses

			// Habitability calculations
			// Determine if planet can retain atmosphere based on mass and distance
			// Small planets and those too close to star struggle to maintain atmospheres
			bool isGasGiant = mass >= 8f && mass <= 10f;
			bool hasAtmosphere = mass >= 4f &&
							   orbitRadius > Properties.Radius * 5f && !isGasGiant;

			bool hasWater = mass >= 0.3f &&
						   orbitRadius >= habitableZoneInner &&
						   orbitRadius <= habitableZoneOuter && !isGasGiant;

			List<Properties.Type> planetResources = new List<Properties.Type>();

			if (isGasGiant)
			{
				planetResources.Add(global::Properties.Type.Hydrogen);
				planetResources.Add(global::Properties.Type.Helium);
			}
			else
			{
				planetResources.AddRange(Properties.SystemResources);
			}
			// Create a new planet properties instance
			PlanetProperties planetProperties = new PlanetProperties
			{
				Mass = mass,
				Radius = 10 + (mass * 3), // Radius scales with mass
				OrbitRadius = orbitRadius,
				OrbitPeriod = new RandomNumberGenerator().RandfRange(0.1f, 1f) * 10,
				RotationPeriod = 24,
				HasAtmosphere = hasAtmosphere,
				HasWater = hasWater,
				Habitability = 0.0f,
				PlanetResources = planetResources,
				IsGasGiant = isGasGiant,
				Moons = new RandomNumberGenerator().RandiRange(0, 2) // Random number of moons
			};

			GD.Print($"GasGiant: {isGasGiant}, hasAtmosphere: {hasAtmosphere}, hasWater: {hasWater}");
			// Create a new planet
			Planet planet = new Planet(planetProperties);
			// Add the planet to the star
			AddChild(planet);
		}
	}
	#endregion
	#region Generate Astroids
	public void GenerateAstroids()
	{
		// Generate asteroids in the asteroid belt
		float orbitRadius = (Properties.Planets + 1) * 100f; // in AU
		Node2D asteroidBelt = new Node2D();
		AddChild(asteroidBelt);
		var random = new RandomNumberGenerator();
		for (int i = 0; i < random.RandiRange(100, 150); i++)
		{
			float mass = random.RandfRange(0.1f, 10f); // Random mass between 0.1 and 10 Earth masses
			Properties.Type type = global::Properties.Type.Rock; // Default type
			if (Properties.SystemResources != null && Properties.SystemResources.Count > 0)
			{
				type = Properties.SystemResources[random.RandiRange(0, Properties.SystemResources.Count - 1)];
			}
			// Create a new planet properties instance
			AstroidProperties astroidProperties = new AstroidProperties
			{
				Mass = mass,
				Radius = 2.5f + (mass), // Radius scales with mass
				AstroidType = type
			};
			// Create a new asteroid
			Astroid astroid = new Astroid(astroidProperties);
			// Set the position of the asteroid
			float beltWidth = 50f; // Width of the asteroid belt
			float randomRadius = orbitRadius + random.RandfRange(-beltWidth, beltWidth * 2);
			astroid.Position = new Vector2(randomRadius, 0).Rotated(Mathf.DegToRad(random.RandfRange(0, 360)));
			// Add the asteroid to the asteroid belt
			asteroidBelt.AddChild(astroid);
		}

	}
	#endregion
}


