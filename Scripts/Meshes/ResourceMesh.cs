using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ResourceMesh : MeshInstance2D
{
    [Export] public Color PrimaryColor = new Color(0.6f, 0.8f, 1.0f); // Main resource color
    [Export] public float Scale = 0.5f; // Overall scale of the resource
    [Export] public bool AddRandomRotation = false; // Whether to randomize orientation
    [Export] public float VariationAmount = 0.5f; // How much to vary the shape (0-1)

    private Random random = new Random();

    public override void _Ready()
    {
        GenerateDodecahedronMesh();
    }

    private void GenerateDodecahedronMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Color> colors = new List<Color>();

        // Golden ratio - key to generating dodecahedron vertices
        float phi = (1 + Mathf.Sqrt(5)) / 2;

        // Generate the vertices of a dodecahedron
        List<Vector3> dodecahedronVertices = new List<Vector3>
    {
        // Create the 20 vertices of a dodecahedron
        new Vector3(1, 1, 1),
        new Vector3(1, 1, -1),
        new Vector3(1, -1, 1),
        new Vector3(1, -1, -1),
        new Vector3(-1, 1, 1),
        new Vector3(-1, 1, -1),
        new Vector3(-1, -1, 1),
        new Vector3(-1, -1, -1),

        new Vector3(0, phi, 1/phi),
        new Vector3(0, phi, -1/phi),
        new Vector3(0, -phi, 1/phi),
        new Vector3(0, -phi, -1/phi),

        new Vector3(1/phi, 0, phi),
        new Vector3(1/phi, 0, -phi),
        new Vector3(-1/phi, 0, phi),
        new Vector3(-1/phi, 0, -phi),

        new Vector3(phi, 1/phi, 0),
        new Vector3(phi, -1/phi, 0),
        new Vector3(-phi, 1/phi, 0),
        new Vector3(-phi, -1/phi, 0)
    };

        // Scale all vertices
        for (int i = 0; i < dodecahedronVertices.Count; i++)
        {
            dodecahedronVertices[i] *= Scale;
        }

        // Define the 12 pentagonal faces of a dodecahedron by vertex indices
        int[][] faces = new int[][] {
        new int[] { 0, 8, 9, 1, 16 },
        new int[] { 0, 16, 17, 2, 12 },
        new int[] { 0, 12, 14, 4, 8 },
        new int[] { 1, 9, 5, 15, 13 },
        new int[] { 1, 13, 3, 17, 16 },
        new int[] { 2, 17, 3, 11, 10 },
        new int[] { 2, 10, 6, 14, 12 },
        new int[] { 3, 13, 15, 7, 11 },
        new int[] { 4, 14, 6, 19, 18 },
        new int[] { 4, 18, 5, 9, 8 },
        new int[] { 5, 18, 19, 7, 15 },
        new int[] { 6, 10, 11, 7, 19 }
    };

        // Define colors for each face with variations based on PrimaryColor
        Color[] faceColors = new Color[faces.Length];

        for (int i = 0; i < faces.Length; i++)
        {
            // Generate a variation factor for this face
            float variationFactor = 1.0f;

            // Apply slight random variation to lighting based on VariationAmount
            if (VariationAmount > 0)
            {
                // Create a lighting factor between 0.7 and 1.3 for natural variation
                variationFactor = 0.7f + (0.6f * (float)random.NextDouble());
            }

            // Create the color for this face
            faceColors[i] = new Color(
                Mathf.Clamp(PrimaryColor.R * variationFactor, 0, 1),
                Mathf.Clamp(PrimaryColor.G * variationFactor, 0, 1),
                Mathf.Clamp(PrimaryColor.B * variationFactor, 0, 1),
                PrimaryColor.A
            );
        }

        // Project the 3D vertices to 2D (simple isometric projection)
        List<Vector3> projectedVertices = new List<Vector3>();
        foreach (Vector3 vertex in dodecahedronVertices)
        {
            // Apply a rotation for better viewing angle
            float xRot = vertex.X * Mathf.Cos(0.3f) - vertex.Z * Mathf.Sin(0.3f);
            float yRot = vertex.Y;
            float zRot = vertex.X * Mathf.Sin(0.3f) + vertex.Z * Mathf.Cos(0.3f);

            // Project to 2D
            float x = xRot;
            float y = yRot * 0.86f - zRot * 0.5f;

            projectedVertices.Add(new Vector3(x, y, 0));
        }

        // Triangulate each face
        for (int f = 0; f < faces.Length; f++)
        {
            int[] face = faces[f];
            Color faceColor = faceColors[f];

            // Find the center of the face
            Vector3 center = new Vector3(0, 0, 0);
            foreach (int idx in face)
            {
                center += projectedVertices[idx];
            }
            center /= face.Length;

            // Create triangles by connecting each edge to the center
            for (int i = 0; i < face.Length; i++)
            {
                int v1 = face[i];
                int v2 = face[(i + 1) % face.Length];

                // Add a triangle
                AddTriangle(vertices, colors,
                            center,
                            projectedVertices[v1],
                            projectedVertices[v2],
                            faceColor);
            }
        }

        // Create the mesh
        ArrayMesh arrayMesh = new ArrayMesh();
        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = vertices.ToArray();
        arrays[(int)Mesh.ArrayType.Color] = colors.ToArray();

        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        Mesh = arrayMesh;

        // Apply random rotation if specified
        if (AddRandomRotation)
        {
            RotationDegrees = (float)random.NextDouble() * 360f;
        }
    }

    private void AddTriangle(List<Vector3> verts, List<Color> colors,
                             Vector3 v1, Vector3 v2, Vector3 v3, Color baseColor)
    {
        verts.Add(v1);
        verts.Add(v2);
        verts.Add(v3);

        // Apply slight color variation to create depth effect
        Color triangleColor = baseColor;
        if (VariationAmount > 0 && random.NextDouble() < 0.4)
        {
            // Darken or lighten the color slightly
            float factor = 1 + ((float)random.NextDouble() - 0.5f) * VariationAmount * 0.4f;
            triangleColor = new Color(
                Mathf.Clamp(baseColor.R * factor, 0, 1),
                Mathf.Clamp(baseColor.G * factor, 0, 1),
                Mathf.Clamp(baseColor.B * factor, 0, 1),
                baseColor.A
            );
        }

        colors.Add(triangleColor);
        colors.Add(triangleColor);
        colors.Add(triangleColor);
    }
    // Helper methods
    private int pointsInRing(int ring, int sides) => ring == 0 ? 1 : ring * sides;

    private int ringStartIndex(int ring, int sides)
    {
        if (ring == 0) return 0;
        return 1 + sides * ring * (ring - 1) / 2;
    }
}