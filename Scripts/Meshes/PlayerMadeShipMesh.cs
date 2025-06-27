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
    public override List<Vector3> DefineVertices()
    {
        
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