using Godot;
using System;

public partial class PlayerCreatedShip : Ship
{
    [Export] public ShipSave shipSave;
    [Export] public string shipPath;
     public override void _Ready()
    {

        Mesh = new PlayerMadeShipMesh(shipSave);
        
        AddChild(Mesh);
        
        base._Ready();
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
    }
    
}
