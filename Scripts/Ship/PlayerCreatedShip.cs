using Godot;
using System;

public partial class PlayerCreatedShip : Ship
{
    [Export] public ShipSave shipSave;
    [Export] public string shipPath;
     public override void _Ready()
    {
        base._Ready();
        Mesh = new PlayerMadeShipMesh(shipSave);

        Mesh.Scale = 100;
        AddChild(Mesh);
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
    }
    
}
