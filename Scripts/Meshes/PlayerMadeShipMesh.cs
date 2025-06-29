using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


public partial class PlayerMadeShipMesh :ShipMesh
{
    private ShipSave shipSave;
    private List<Vector3> centeredVertices = null;
    private List<AttachmentPoint> shipNodes = new();
    public PlayerMadeShipMesh(ShipSave shipSave)
    {
        this.shipSave = shipSave;
        CreateAttachmentPoints();
    }
    public PlayerMadeShipMesh(string saveName)
    {
        shipSave = (ShipSave)ResourceLoader.Load(saveName);
    }

    public void CreateAttachmentPoints()
    {
        UseMean(shipSave.NodePositions.ToList());
        foreach (var pos in centeredVertices)
        {
            var ap = new AttachmentPoint();
            ap.Position = new Vector2(pos.X, pos.Y);
            ap.Name = "AttachmentPoint_" + shipNodes.Count;
            ap.NodeType = (AttachmentPoint.ShipNodeTypes)shipSave.NodeTypes[shipNodes.Count];
            shipNodes.Add(ap);
        }
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
        if (centeredVertices != null)
        {
            return centeredVertices;
        }
        // Calculate the actual geometric center of all vertices
        Vector3 geometricCenter = Vector3.Zero;
        foreach (var vertex in vertices)
        {
            geometricCenter += vertex;
        }
        geometricCenter /= vertices.Count;
    
        // Subtract the geometric center from each vertex to center around (0,0,0)
        centeredVertices = new List<Vector3>();
        foreach (var vertex in vertices)
        {
            centeredVertices.Add(vertex - geometricCenter);
        }
    
        return centeredVertices;
    }

    public List<AttachmentPoint> GetShipNodes()
    {
        return shipNodes;
    }
}