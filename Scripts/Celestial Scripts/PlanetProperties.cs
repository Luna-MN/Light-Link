using Godot;
using System;

public partial class PlanetProperties
{
    public float Mass; // in Earth Masses
    public float Radius; // in Earth Radii
    public float OrbitRadius; // in AU (Astronomical Units)
    public float OrbitPeriod; // in Earth days
    public float RotationPeriod; // in Earth hours
    public float AxialTilt; // in degrees
    public Color ColorIndex; // Color index for the planet
    public bool HasAtmosphere; // Whether the planet has an atmosphere or not
    public bool HasWater; // Whether the planet has water or not
}
