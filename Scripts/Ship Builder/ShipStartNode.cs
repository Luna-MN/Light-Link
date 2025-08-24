using Godot;
using System;
[GlobalClass]
public partial class ShipStartNode() : ShipNode(ShipBuilder.ShipNodeTypes.Start)
{   
    public override void _Ready()
    {
        base._Ready();
        shipMesh.Mesh = new PrismMesh();
        shipMesh.RotationDegrees = 180;
        Modulate = Colors.Purple;
    }
}
