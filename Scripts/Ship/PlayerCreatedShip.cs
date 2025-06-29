using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerCreatedShip : Ship
{
    [Export] public ShipSave shipSave;
    [Export] public string shipPath;
    private List<AttachmentPoint> shipNodes = new();
     public override void _Ready()
    {

        Mesh = new PlayerMadeShipMesh(shipSave);
        shipNodes = ((PlayerMadeShipMesh)Mesh).GetShipNodes();
        
        AddChild(Mesh);
        base._Ready();
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
    }
    
}
