using Godot;
using System;
using System.Collections.Generic;

public partial class StarProperties : Properties
{
    public float Tempreture;
    public float Luminosity;
    public int Planets;
    public List<AstroidProperties.AstroidType> SystemResources = new List<AstroidProperties.AstroidType>();
    public void SetResorces()
    {
        // Set the resources of the star system based on its properties
        SystemResources.Add(AstroidProperties.AstroidType.Iron);
        SystemResources.Add(AstroidProperties.AstroidType.Ice);
        SystemResources.Add(AstroidProperties.AstroidType.Rock);
        if (Luminosity > 1.0f)
        {
            SystemResources.Add(AstroidProperties.AstroidType.Gold);
            SystemResources.Add(AstroidProperties.AstroidType.Silver);
        }
        else if (Luminosity < 0.5f)
        {
            SystemResources.Add(AstroidProperties.AstroidType.Copper);
        }
        else
        {
            SystemResources.Add(AstroidProperties.AstroidType.Carbon);
            SystemResources.Add(AstroidProperties.AstroidType.Silicon);
        }

    }
}