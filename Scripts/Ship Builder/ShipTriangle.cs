
using Godot;

public partial class ShipTriangle
{
    public ShipNode point1;
    public ShipNode point2;
    public ShipNode point3;
    public ArrayMesh mesh;
    public MeshInstance2D meshInstance;

    public ShipTriangle(ShipNode p1, ShipNode p2, ShipNode p3, ShipBuilder shipBuilder)
    {
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
        shipBuilder.AddChild(meshInstance);

    }

}
