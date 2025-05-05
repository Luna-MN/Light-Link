using Godot;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public partial class Star : Body
{
	// Properties of the star
	public StarProperties Properties;
	public StarUI starUI;
	// Constructor
	public Star(Vector2 pos, StarProperties properties, MeshType type = MeshType.Star) : base(type)
	{
		// Set the position of the star
		Properties = properties;
		SetStarProperties();
		UpdateRBScale(Properties.Radius);
		Position = pos;
		Properties.Position = pos;
		if (Mesh is LowPolyStarMesh starMesh)
		{
			starMesh.GenerateStar(Properties.Temperature, 0.6f);
		}
		else
		{
			// Create the correct mesh type
			Mesh = new LowPolyStarMesh();
			((LowPolyStarMesh)Mesh).GenerateStar(Properties.Temperature, 0.6f);
		}
		Mesh.Scale = new Vector2(Properties.Radius, Properties.Radius);
		// Apply star-specific visual properties
		ShaderMaterial material = Mesh.Material as ShaderMaterial;
		Properties.SetResorces();
		GeneratePlanets();

		GenerateAstroids();
		CreateStarUI();
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		starUI.SetStarProperties(Properties);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	#region Star Properties
	public void SetStarProperties()
	{
		var random = new RandomNumberGenerator();
		random.Randomize();

		// First determine star type based on temperature
		if (Properties.Temperature <= 3200)
		{
			Properties.SType = StarProperties.StarType.M;
			// M-type: red dwarf
			Properties.Radius = random.RandfRange(0.1f, 0.5f);
			// For M-type stars: Mass ≈ Radius^1.25 (rough approximation)
			Properties.Mass = Mathf.Pow(Properties.Radius, 1.25f);
			// Ensure mass is in expected range for M-type stars (0.08-0.45 solar masses)
			Properties.Mass = Mathf.Clamp(Properties.Mass, 0.08f, 0.45f);

			Properties.Luminosity = random.RandfRange(0.001f, 0.08f);
			Properties.Planets = random.RandiRange(1, 4);
			Properties.ColorIndex = new Color(1.0f, 0.4f, 0.4f); // Reddish
		}
		else if (Properties.Temperature <= 4000)
		{
			Properties.SType = StarProperties.StarType.K;
			// K-type: orange dwarf
			Properties.Radius = random.RandfRange(0.5f, 0.8f);
			// For K-type stars: Mass ≈ Radius^1.25 (rough approximation)
			Properties.Mass = Mathf.Pow(Properties.Radius, 1.25f);
			// Ensure mass is in expected range for K-type stars (0.45-0.8 solar masses)
			Properties.Mass = Mathf.Clamp(Properties.Mass, 0.45f, 0.8f);

			Properties.Luminosity = random.RandfRange(0.08f, 0.6f);
			Properties.Planets = random.RandiRange(2, 6);
			Properties.ColorIndex = new Color(1.0f, 0.6f, 0.4f); // Orange
		}
		else if (Properties.Temperature <= 5200)
		{
			Properties.SType = StarProperties.StarType.KG;
			// KG-type: boundary between K and G
			Properties.Radius = random.RandfRange(0.8f, 0.95f);
			// For KG-type stars: Mass ≈ Radius (nearly linear relationship)
			Properties.Mass = Properties.Radius * 1.05f; // Slight adjustment for appropriate range
														 // Ensure mass is in expected range (0.8-0.95 solar masses)
			Properties.Mass = Mathf.Clamp(Properties.Mass, 0.8f, 0.95f);

			Properties.Luminosity = random.RandfRange(0.6f, 0.9f);
			Properties.Planets = random.RandiRange(3, 8);
			Properties.ColorIndex = new Color(1.0f, 0.8f, 0.6f); // Yellow-orange
		}
		else if (Properties.Temperature < 6000)
		{
			Properties.SType = StarProperties.StarType.G;
			// G-type: yellow (Sun-like)
			Properties.Radius = random.RandfRange(0.9f, 1.1f);
			// For G-type stars: Mass ≈ Radius (linear relationship)
			Properties.Mass = Properties.Radius;
			// Ensure mass is in expected range for G-type stars (0.8-1.2 solar masses)
			Properties.Mass = Mathf.Clamp(Properties.Mass, 0.9f, 1.2f);

			Properties.Luminosity = random.RandfRange(0.6f, 1.5f);
			Properties.Planets = random.RandiRange(3, 10);
			Properties.ColorIndex = new Color(1.0f, 1.0f, 0.8f); // Yellow
		}
		else if (Properties.Temperature < 7200)
		{
			Properties.SType = StarProperties.StarType.F;
			// F-type: yellow-white
			Properties.Radius = random.RandfRange(1.1f, 1.4f);
			// For F-type stars: Mass ≈ Radius^1.5 (rough approximation)
			Properties.Mass = Mathf.Pow(Properties.Radius, 1.5f);
			// Ensure mass is in expected range for F-type stars (1.2-1.5 solar masses)
			Properties.Mass = Mathf.Clamp(Properties.Mass, 1.2f, 1.5f);

			Properties.Luminosity = random.RandfRange(1.5f, 5.0f);
			Properties.Planets = random.RandiRange(2, 8);
			Properties.ColorIndex = new Color(1.0f, 1.0f, 0.9f); // Yellow-white
		}
		else if (Properties.Temperature < 10000)
		{
			Properties.SType = StarProperties.StarType.A;
			// A-type: white
			Properties.Radius = random.RandfRange(1.4f, 2.1f);
			// For A-type stars: Mass ≈ Radius^1.67 (rough approximation)
			Properties.Mass = Mathf.Pow(Properties.Radius, 1.67f);
			// Ensure mass is in expected range for A-type stars (1.5-2.5 solar masses)
			Properties.Mass = Mathf.Clamp(Properties.Mass, 1.5f, 2.5f);

			Properties.Luminosity = random.RandfRange(5.0f, 25.0f);
			Properties.Planets = random.RandiRange(1, 4);
			Properties.ColorIndex = new Color(0.9f, 0.9f, 1.0f); // White with blue tint
		}
		else if (Properties.Temperature < 30000)
		{
			Properties.SType = StarProperties.StarType.B;
			// B-type: blue-white
			Properties.Radius = random.RandfRange(2.1f, 7.0f);
			// For B-type stars: Mass ≈ Radius^1.67 (rough approximation)
			Properties.Mass = Mathf.Pow(Properties.Radius, 1.67f);
			// Ensure mass is in expected range for B-type stars (2.5-10 solar masses)
			Properties.Mass = Mathf.Clamp(Properties.Mass, 2.5f, 10.0f);

			Properties.Luminosity = random.RandfRange(25.0f, 30000.0f);
			Properties.Planets = random.RandiRange(0, 2);
			Properties.ColorIndex = new Color(0.8f, 0.8f, 1.0f); // Blue-white
		}
		else
		{
			Properties.SType = StarProperties.StarType.O;
			// O-type: blue, hottest and brightest
			Properties.Radius = random.RandfRange(7.0f, 15.0f);
			// For O-type stars: Mass ≈ Radius^1.8 (rough approximation for massive stars)
			Properties.Mass = Mathf.Pow(Properties.Radius, 1.8f);
			// Ensure mass is in expected range for O-type stars (10-150 solar masses)
			Properties.Mass = Mathf.Clamp(Properties.Mass, 10.0f, 150.0f);

			Properties.Luminosity = random.RandfRange(30000.0f, 1000000.0f);
			Properties.Planets = random.RandiRange(0, 1); // Rarely have planets
			Properties.ColorIndex = new Color(0.6f, 0.6f, 1.0f); // Blue
		}

		// Convert radius from solar radii to game units
		Properties.Radius *= 50.0f; // Assuming 1 solar radius = 50 game units
		Mesh.Scale = new Vector2(Properties.Radius, Properties.Radius);

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
			float orbitRadius = ((i + 1) * 100f) + Properties.Radius; // in AU
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
		float orbitRadius = ((Properties.Planets + 1) * 100f) + Properties.Radius; // in AU
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
	#region Star UI
	public void CreateStarUI()
	{
		// Create a new star UI instance
		starUI = new StarUI();
		// Add the star UI to the scene
		AddChild(starUI);
		// Set the star properties

	}
	#endregion
}


