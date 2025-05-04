using Godot;
using System;
using System.Collections.Generic;

public partial class StarProperties : Properties
{
    public float Tempreture;
    public float Luminosity;
    public int Planets;
    public List<Type> SystemResources = new List<Type>();
    public enum StarType
    {
        M,
        K,
        KG,
        G,
        F,
        A,
        B,
        O
    }
    public StarType SType;
    public void SetResorces()
    {
        // Set the resources of the star system based on its properties
        SystemResources.Add(Type.Iron);
        SystemResources.Add(Type.Ice);
        SystemResources.Add(Type.Rock);
        if (Luminosity > 1.0f)
        {
            SystemResources.Add(Type.Gold);
            SystemResources.Add(Type.Silver);
        }
        else if (Luminosity < 0.5f)
        {
            SystemResources.Add(Type.Copper);
        }
        else
        {
            SystemResources.Add(Type.Carbon);
            SystemResources.Add(Type.Silicon);
        }

    }
}