using Godot;
using System;

public partial class PlanetProperties : Properties
{
    public float OrbitRadius; // in AU (Astronomical Units)
    public float OrbitPeriod; // in Earth days
    public float RotationPeriod; // in Earth hours
    public bool HasAtmosphere; // Whether the planet has an atmosphere or not
    public bool HasWater; // Whether the planet has water or not
    public float WaterAmount = 0.0f; // 0.0 to 1.0 
}
