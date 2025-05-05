using Godot;
using System;
using System.Collections.Generic;

public partial class PlanetProperties : Properties
{
    public PlanetProperties()
    {
        SetHabitability();
        if (HasWater)
        {
            WaterAmount = 0.25f; // Default water amount
            if (HasAtmosphere)
            {
                WaterAmount = 0.8f; // Higher water amount if atmosphere is present
            }
        }

    }
    public string Name;
    public float OrbitRadius; // in AU (Astronomical Units)
    public float OrbitPeriod; // in Earth days
    public float RotationPeriod; // in Earth hours
    public bool IsGasGiant;
    public bool HasAtmosphere; // Whether the planet has an atmosphere or not
    public bool HasWater; // Whether the planet has water or not
    public float WaterAmount = 0.6f; // 0.0 to 1.0 
    public float Habitability; // 0.0 to 1.0
    public List<Type> PlanetResources = new List<Type>(); // Resources available on the planet
    public int Moons; // Number of moons the planet has
    private void SetHabitability()
    {
        // Calculate habitability based on properties
        Habitability = 0.0f;
        if (HasAtmosphere)
        {
            Habitability += 0.2f; // Atmosphere is essential for life
        }
        if (HasWater)
        {
            Habitability += 0.4f; // Water is essential for life
        }
        if (WaterAmount > 0.5f)
        {
            Habitability += 0.1f; // Higher water amount increases habitability
        }
        if (RotationPeriod == 0)
        {
            Habitability = 0.0f; // No rotation means no habitability
        }
        if (RotationPeriod < 24)
        {
            Habitability += 0.1f; // Faster rotation is better for habitability
        }
    }
}
