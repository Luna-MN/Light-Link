using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


public partial class PlayerMadeShipMesh :ShipMesh
{
    private ShipSave shipSave;
    public PlayerMadeShipMesh(ShipSave shipSave)
    {
        this.shipSave = shipSave;
    }
    public PlayerMadeShipMesh(string saveName)
    {
        shipSave = (ShipSave)ResourceLoader.Load(saveName);
    }
    public override List<Vector3> DefineVertices()
    {
        var vertices = shipSave.NodePositions.ToList();
        GD.Print($"Vertices count: {vertices.Count}");

        return shipSave.NodePositions.ToList();
    }
    protected override List<int> DefineTriangles()
    {
        
        return shipSave.DefineTriangles.ToList();
    }
    protected override List<Color> DefineColors()
    {
        return shipSave.TriangleColors.ToList();
    }
    
}