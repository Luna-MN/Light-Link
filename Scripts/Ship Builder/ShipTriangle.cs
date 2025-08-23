using System.Collections.Generic;
using Godot;
[GlobalClass]
public partial class ShipTriangle : Node2D
{
    public ShipNode point1;
    public ShipNode point2;
    public ShipNode point3;
    public ArrayMesh mesh;
    public MeshInstance2D meshInstance;
    public Area2D area;
    public CollisionShape2D shape;
    public List<ShipLine> lines;

    public ShipTriangle(ShipNode p1, ShipNode p2, ShipNode p3, ShipBuilder shipBuilder, List<ShipLine> lines)
    {
        this.lines = lines;
        
        Name = "ShipTriangle";
        shipBuilder.GetNode("Triangles").AddChild(this);

        point1 = p1;
        point2 = p2;
        point3 = p3;

        mesh = new ArrayMesh();
        var surfaceArray = new Godot.Collections.Array();
        surfaceArray.Resize((int)Mesh.ArrayType.Max);
        surfaceArray[(int)Mesh.ArrayType.Vertex] = new Vector2[]
        {
            point1.GlobalPosition,
            point2.GlobalPosition,
            point3.GlobalPosition
        };
        surfaceArray[(int)Mesh.ArrayType.Index] = new int[] { 0, 1, 2 };
        surfaceArray[(int)Mesh.ArrayType.Color] = new Color[]
        {
            Colors.White,
            Colors.White,
            Colors.White
        };
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
        meshInstance = new MeshInstance2D();
        meshInstance.Mesh = mesh;
        AddChild(meshInstance);

        area = new Area2D();
        shape = new CollisionShape2D();
        shape.Shape = new ConvexPolygonShape2D
        {
            Points = new Vector2[]
            {
                point1.GlobalPosition,
                point2.GlobalPosition,
                point3.GlobalPosition
            }
        };
        area.AddChild(shape);
        AddChild(area);
    }
    public void UpdateTriangle()
    {
        if (meshInstance != null && mesh != null)
        {
            // Update the visual mesh
            var surfaceArray = new Godot.Collections.Array();
            surfaceArray.Resize((int)Mesh.ArrayType.Max);
            surfaceArray[(int)Mesh.ArrayType.Vertex] = new Vector2[]
            {
                point1.GlobalPosition,
                point2.GlobalPosition,
                point3.GlobalPosition
            };
            surfaceArray[(int)Mesh.ArrayType.Index] = new int[] { 0, 1, 2 };
            surfaceArray[(int)Mesh.ArrayType.Color] = new Color[]
            {
                Colors.White,
                Colors.White,
                Colors.White
            };
            mesh.ClearSurfaces();
            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
        }

        // Update the collision shape
        if (shape != null && shape.Shape is ConvexPolygonShape2D polygonShape)
        {
            polygonShape.Points = new Vector2[]
            {
                point1.GlobalPosition,
                point2.GlobalPosition,
                point3.GlobalPosition
            };
        }
    }
}
