using Godot;
using System;
using System.Collections.Generic;

public partial class LowPolyPlanetMesh : MeshInstance2D // Changed from MeshInstance2D to MeshInstance3D
{
    [Export] public float Radius = 0.5f;
    [Export] public int Subdivisions = 2;
    [Export] public Color DefaultColor = new Color(1.0f, 0.5f, 0.5f); // Default color for triangles

    private List<Color> triangleColors = new List<Color>(); // Store colors for each triangle
    private bool useVertexColors = false;

    public void GeneratePlanetMesh()
    {
        ArrayMesh mesh = new ArrayMesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> indices = new List<int>();
        List<Color> vertexColors = new List<Color>();

        // Create an icosahedron as the base shape (20-sided polyhedron)
        GenerateIcosahedron(vertices, indices);

        // Subdivide the faces to get more triangles
        for (int i = 0; i < Subdivisions; i++)
        {
            SubdivideIcosahedron(vertices, indices);
        }

        // Project vertices onto sphere and calculate normals
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = vertices[i].Normalized() * Radius;
            normals.Add(vertices[i].Normalized());
        }

        // For sharp color transitions, we need to duplicate vertices
        // and assign colors per-triangle instead of per-vertex
        List<Vector3> sharpVertices = new List<Vector3>();
        List<Vector3> sharpNormals = new List<Vector3>();
        List<Color> sharpColors = new List<Color>();
        List<int> sharpIndices = new List<int>();

        // Process each triangle separately
        for (int i = 0; i < indices.Count; i += 3)
        {
            int triangleIndex = i / 3;
            Color color = DefaultColor;

            if (useVertexColors && triangleColors.Count > 0 && triangleIndex < triangleColors.Count)
            {
                color = triangleColors[triangleIndex];
            }

            // Add three vertices for this triangle (duplicating them to avoid color interpolation)
            int baseIndex = sharpVertices.Count;

            // First vertex
            sharpVertices.Add(vertices[indices[i]]);
            sharpNormals.Add(normals[indices[i]]);
            sharpColors.Add(color);

            // Second vertex
            sharpVertices.Add(vertices[indices[i + 1]]);
            sharpNormals.Add(normals[indices[i + 1]]);
            sharpColors.Add(color);

            // Third vertex
            sharpVertices.Add(vertices[indices[i + 2]]);
            sharpNormals.Add(normals[indices[i + 2]]);
            sharpColors.Add(color);

            // Add indices for this triangle
            sharpIndices.Add(baseIndex);
            sharpIndices.Add(baseIndex + 1);
            sharpIndices.Add(baseIndex + 2);
        }

        // Create surface arrays with the sharp (non-interpolated) data
        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = sharpVertices.ToArray();
        arrays[(int)Mesh.ArrayType.Normal] = sharpNormals.ToArray();
        arrays[(int)Mesh.ArrayType.Color] = sharpColors.ToArray();
        arrays[(int)Mesh.ArrayType.Index] = sharpIndices.ToArray();

        // Create the mesh
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        // Create and apply material
        StandardMaterial3D material = new StandardMaterial3D();
        material.VertexColorUseAsAlbedo = true; // Use vertex colors
        material.Roughness = 0.7f;

        // Set the mesh and material
        this.Mesh = mesh;
        this.Material = material;
    }

    // New method to set triangle colors
    public void SetTriangleColors(List<Color> colors)
    {
        triangleColors = colors;
        useVertexColors = true;
        GeneratePlanetMesh(); // Rebuild the mesh with the new colors
    }

    // New method to set a specific triangle's color
    public void SetTriangleColor(int triangleIndex, Color color)
    {
        if (triangleColors.Count <= triangleIndex)
        {
            // Fill with default color up to the index we want
            while (triangleColors.Count < triangleIndex)
            {
                triangleColors.Add(DefaultColor);
            }
            triangleColors.Add(color);
        }
        else
        {
            triangleColors[triangleIndex] = color;
        }

        useVertexColors = true;
        GeneratePlanetMesh();
    }

    // New method to randomly color triangles (for testing)
    public void RandomizeTriangleColors()
    {
        triangleColors.Clear();
        Random random = new Random();

        // Get the number of triangles (indices / 3)
        int triangleCount = GetTriangleCount();

        for (int i = 0; i < triangleCount; i++)
        {
            triangleColors.Add(new Color(
                (float)random.NextDouble(),
                (float)random.NextDouble(),
                (float)random.NextDouble()
            ));
        }

        useVertexColors = true;
        GeneratePlanetMesh();
    }

    // Get the current count of triangles in the mesh
    public int GetTriangleCount()
    {
        if (Mesh is ArrayMesh arrayMesh && arrayMesh.GetSurfaceCount() > 0)
        {
            Godot.Collections.Array arrays = arrayMesh.SurfaceGetArrays(0);
            int[] indices = (int[])arrays[(int)Mesh.ArrayType.Index];
            return indices.Length / 3;
        }

        // If no mesh yet, calculate the expected triangle count for an icosahedron with subdivisions
        int faceCount = 20; // Icosahedron starts with 20 faces
        for (int i = 0; i < Subdivisions; i++)
        {
            faceCount *= 4; // Each subdivision multiplies faces by 4
        }

        return faceCount;
    }

    private void GenerateIcosahedron(List<Vector3> vertices, List<int> indices)
    {
        // Golden ratio for icosahedron construction
        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        // Create the 12 vertices of the icosahedron
        vertices.Add(new Vector3(-1, t, 0).Normalized());
        vertices.Add(new Vector3(1, t, 0).Normalized());
        vertices.Add(new Vector3(-1, -t, 0).Normalized());
        vertices.Add(new Vector3(1, -t, 0).Normalized());

        vertices.Add(new Vector3(0, -1, t).Normalized());
        vertices.Add(new Vector3(0, 1, t).Normalized());
        vertices.Add(new Vector3(0, -1, -t).Normalized());
        vertices.Add(new Vector3(0, 1, -t).Normalized());

        vertices.Add(new Vector3(t, 0, -1).Normalized());
        vertices.Add(new Vector3(t, 0, 1).Normalized());
        vertices.Add(new Vector3(-t, 0, -1).Normalized());
        vertices.Add(new Vector3(-t, 0, 1).Normalized());

        // Define the 20 triangular faces of the icosahedron
        AddTriangle(indices, 0, 11, 5);
        AddTriangle(indices, 0, 5, 1);
        AddTriangle(indices, 0, 1, 7);
        AddTriangle(indices, 0, 7, 10);
        AddTriangle(indices, 0, 10, 11);

        AddTriangle(indices, 1, 5, 9);
        AddTriangle(indices, 5, 11, 4);
        AddTriangle(indices, 11, 10, 2);
        AddTriangle(indices, 10, 7, 6);
        AddTriangle(indices, 7, 1, 8);

        AddTriangle(indices, 3, 9, 4);
        AddTriangle(indices, 3, 4, 2);
        AddTriangle(indices, 3, 2, 6);
        AddTriangle(indices, 3, 6, 8);
        AddTriangle(indices, 3, 8, 9);

        AddTriangle(indices, 4, 9, 5);
        AddTriangle(indices, 2, 4, 11);
        AddTriangle(indices, 6, 2, 10);
        AddTriangle(indices, 8, 6, 7);
        AddTriangle(indices, 9, 8, 1);
    }

    private void AddTriangle(List<int> indices, int a, int b, int c)
    {
        indices.Add(a);
        indices.Add(b);
        indices.Add(c);
    }

    private void SubdivideIcosahedron(List<Vector3> vertices, List<int> indices)
    {
        List<int> newIndices = new List<int>();
        Dictionary<long, int> midpointCache = new Dictionary<long, int>();

        for (int i = 0; i < indices.Count; i += 3)
        {
            int v1 = indices[i];
            int v2 = indices[i + 1];
            int v3 = indices[i + 2];

            int a = GetMidpoint(midpointCache, vertices, v1, v2);
            int b = GetMidpoint(midpointCache, vertices, v2, v3);
            int c = GetMidpoint(midpointCache, vertices, v3, v1);

            // Create 4 new triangles from the original one
            AddTriangle(newIndices, v1, a, c);
            AddTriangle(newIndices, v2, b, a);
            AddTriangle(newIndices, v3, c, b);
            AddTriangle(newIndices, a, b, c);
        }

        indices.Clear();
        indices.AddRange(newIndices);
    }

    private int GetMidpoint(Dictionary<long, int> cache, List<Vector3> vertices, int i1, int i2)
    {
        // Ensure i1 <= i2 for consistent key generation
        if (i1 > i2)
        {
            int temp = i1;
            i1 = i2;
            i2 = temp;
        }

        // Create a unique key for this edge
        long key = ((long)i1 << 32) | (uint)i2;

        // If we've already calculated this midpoint, return its index
        if (cache.ContainsKey(key))
        {
            return cache[key];
        }

        // Calculate the midpoint
        Vector3 p1 = vertices[i1];
        Vector3 p2 = vertices[i2];
        Vector3 middle = (p1 + p2) * 0.5f;

        // Add the new vertex
        int index = vertices.Count;
        vertices.Add(middle.Normalized());

        // Cache and return the index
        cache[key] = index;
        return index;
    }

    public override void _Ready()
    {
        GeneratePlanetMesh();
    }

    // Updated to include color customization based on properties
    public void ApplyPlanetProperties(PlanetProperties properties)
    {
        // Apply basic properties
        if (properties.HasWater && properties.WaterAmount > 0)
        {
            // Create a gradient from land to water
            List<Color> colors = new List<Color>();

            Color landColor = properties.ColorIndex;
            Color waterColor = new Color(0.2f, 0.4f, 0.8f); // blue water

            int triangleCount = GetTriangleCount();
            Random random = new Random();

            for (int i = 0; i < triangleCount; i++)
            {
                // Random variation to create more interesting patterns
                float variation = (float)random.NextDouble() * 0.15f - 0.075f;

                // Decide if this triangle is water or land based on water coverage and some randomness
                if (random.NextDouble() < properties.WaterAmount + variation)
                {
                    colors.Add(waterColor);
                }
                else
                {
                    colors.Add(landColor);
                }
            }

            SetTriangleColors(colors);
        }
        else
        {
            // Just use the default color if no water
            triangleColors.Clear();
            useVertexColors = false;
            GeneratePlanetMesh();
        }
    }
}