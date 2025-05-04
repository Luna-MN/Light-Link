using Godot;
using System;

public abstract class Properties
{
    public float Mass;
    public float Radius;
    public float Age;
    public Color ColorIndex = new Color(1, 1, 1); // Default color is white
    public enum Type
    {
        Iron,
        Ice,
        Rock,
        Gold,
        Silver,
        Copper,
        Carbon,
        Silicon,
        Hydrogen,
        Helium
    }
}