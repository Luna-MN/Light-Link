using Godot;
using System;

public partial class MainShip : PlayerShips
{
    public override void _Ready()
    {
        // Initialize ship properties
        Mesh = new PlayerShipMesh();
        Mesh.Scale = 25; // Set the scale of the ship
        AddChild(Mesh);


        base._Ready();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        // Additional processing for the main ship
    }
}
