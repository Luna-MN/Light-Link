using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerCreatedShip : PlayerShips
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

    public void ShowNodes()
    {
        foreach (var node in shipNodes)
        {
            node.Visible = true;
        }
    }
}
