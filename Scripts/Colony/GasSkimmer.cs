using Godot;
using System;

public partial class GasSkimmer : Building
{
    public override void _Ready()
    {
        orbitRadius = 10f;
        base._Ready();

        // create the mesh for the gas skimmer
        Mesh = new GasSkimmerMesh();
        Scale = new Vector2(10, 10);
        AddChild(Mesh);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}