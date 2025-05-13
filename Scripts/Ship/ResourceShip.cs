using Godot;
using System;

public partial class ResourceShip : Ship
{
    public override void _Ready()
    {
        base._Ready();
        Mesh = new ResourceShipMesh();
        Scale = new Vector2(10, 10);
        AddChild(Mesh);
    }
}