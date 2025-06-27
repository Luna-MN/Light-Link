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
        vertices = UseMean(vertices);
        return vertices;
    }
    protected override List<int> DefineTriangles()
    {
        var triangles =shipSave.DefineTriangles.ToList();
        GD.Print($"Triangles count: {triangles.Count}");
        
        return triangles;
    }
    protected override List<Color> DefineColors()
    {
        return shipSave.TriangleColors.ToList();
    }

    private List<Vector3> UseMean(List<Vector3> vertices)
    {
        var meanVertices = new List<Vector3>();
        var meanLoc = new Vector3(shipSave.MeanNode.GlobalPosition.X, shipSave.MeanNode.GlobalPosition.Y, 0);
        foreach (var node in vertices)
        {
            meanVertices.Add(node - meanLoc);
        }
        return meanVertices;
    }
}