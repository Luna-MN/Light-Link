using Godot;
using System;

public partial class AstroidProperties : Properties
{
    public enum AstroidType
    {
        Iron,
        Ice,
        Rock,
        Gold,
        Silver,
        Copper,
        Carbon,
        Silicon
    }
    public AstroidType Type;
}