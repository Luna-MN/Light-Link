using Godot;
using System;
[GlobalClass]
public partial class ShipStartNode() : ShipNode(ShipBuilder.ShipNodeTypes.Start)
{
    protected MeshInstance2D shipMeshOutline;
    public override void _Ready()
    {
        base._Ready();
        shipMesh.Mesh = new PrismMesh();
        shipMesh.RotationDegrees = 180;
        
        shipMeshOutline = new MeshInstance2D();
        shipMeshOutline.Mesh = new PrismMesh();
        shipMeshOutline.RotationDegrees = 180;
        shipMeshOutline.ShowBehindParent = true;
        shipMeshOutline.Scale = new Vector2(12.5f, 12.5f);
        shipMeshOutline.Modulate = Colors.LightBlue;
        AddChild(shipMeshOutline);
        
    }
}
