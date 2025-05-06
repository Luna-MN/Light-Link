using Godot;
using System.Collections.Generic;

public partial class ShipMesh : MeshInstance2D
{
    [Export] public Color CockpitColor = new Color(0.2f, 0.6f, 1.0f);
    [Export] public Color BodyColor = new Color(0.8f, 0.8f, 0.8f);
    [Export] public Color WingColor = new Color(0.7f, 0.7f, 0.7f);
    [Export] public float Scale = 1.0f;

    public override void _Ready()
    {
        GenerateShipMesh();
    }

    protected virtual void GenerateShipMesh()
    {
        ArrayMesh mesh = new ArrayMesh();

        // Get ship-specific data from derived classes
        List<Vector3> vertices = DefineVertices();
        List<int> indices = DefineTriangles();
        List<Color> triangleColors = DefineColors();

        // Validate data before proceeding
        if (vertices.Count == 0 || indices.Count == 0 || triangleColors.Count == 0)
        {
            GD.PrintErr("Ship mesh creation failed: One or more required data lists is empty.");
            GD.PrintErr($"Vertices count: {vertices.Count}, Indices count: {indices.Count}, Colors count: {triangleColors.Count}");

            // Create a simple fallback mesh if we're not in a derived class
            if (GetType() == typeof(ShipMesh))
            {
                GD.Print("Using fallback ship mesh");
                CreateFallbackMesh();
            }
            return;
        }

        // Ensure we have enough colors for all triangles
        while (triangleColors.Count < indices.Count / 3)
        {
            triangleColors.Add(BodyColor);
        }

        // Scale vertices
        for (int i = 0; i < vertices.Count; i++)
            vertices[i] *= Scale;

        // Duplicate vertices per triangle for sharp edges
        List<Vector3> sharpVertices = new List<Vector3>();
        List<Color> sharpColors = new List<Color>();
        List<int> sharpIndices = new List<int>();

        for (int i = 0; i < indices.Count; i += 3)
        {
            int triangleIndex = i / 3;
            int baseIndex = sharpVertices.Count;

            sharpVertices.Add(vertices[indices[i]]);
            sharpVertices.Add(vertices[indices[i + 1]]);
            sharpVertices.Add(vertices[indices[i + 2]]);

            // Get the color for this triangle
            Color color = triangleColors[triangleIndex];

            sharpColors.Add(color);
            sharpColors.Add(color);
            sharpColors.Add(color);

            sharpIndices.Add(baseIndex);
            sharpIndices.Add(baseIndex + 1);
            sharpIndices.Add(baseIndex + 2);
        }

        // Create mesh arrays
        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = sharpVertices.ToArray();
        arrays[(int)Mesh.ArrayType.Color] = sharpColors.ToArray();
        arrays[(int)Mesh.ArrayType.Index] = sharpIndices.ToArray();

        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        // Material setup
        StandardMaterial3D material = new StandardMaterial3D();
        material.VertexColorUseAsAlbedo = true;
        material.Roughness = 0.8f;

        this.Mesh = mesh;
        this.Material = material;
    }

    // Fallback mesh for direct instances of ShipMesh
    private void CreateFallbackMesh()
    {
        // Create a very simple triangle mesh as fallback
        List<Vector3> vertices = new List<Vector3>
        {
            new Vector3(0, -0.5f, 0),
            new Vector3(-0.3f, 0.3f, 0),
            new Vector3(0.3f, 0.3f, 0)
        };

        // Scale vertices
        for (int i = 0; i < vertices.Count; i++)
            vertices[i] *= Scale;

        List<int> indices = new List<int> { 0, 1, 2 };
        List<Color> colors = new List<Color> { BodyColor };

        ArrayMesh mesh = new ArrayMesh();
        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = vertices.ToArray();
        arrays[(int)Mesh.ArrayType.Color] = new Color[] { BodyColor, BodyColor, BodyColor };
        arrays[(int)Mesh.ArrayType.Index] = indices.ToArray();

        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        StandardMaterial3D material = new StandardMaterial3D();
        material.VertexColorUseAsAlbedo = true;

        this.Mesh = mesh;
        this.Material = material;
    }

    // Virtual methods to be overridden by derived classes
    protected virtual List<Vector3> DefineVertices()
    {
        // Base implementation returns an empty list
        return new List<Vector3>();
    }

    protected virtual List<int> DefineTriangles()
    {
        // Base implementation returns an empty list
        return new List<int>();
    }

    protected virtual List<Color> DefineColors()
    {
        // Base implementation returns an empty list
        return new List<Color>();
    }
}