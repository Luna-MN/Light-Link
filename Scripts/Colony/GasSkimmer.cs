using Godot;
using System;

public partial class GasSkimmer : Building
{
    public override void _Ready()
    {
        base._Ready();

        // create the mesh for the gas skimmer
        Mesh = new GasSkimmerMesh();
        Scale = new Vector2(50, 50);
        AddChild(Mesh);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}