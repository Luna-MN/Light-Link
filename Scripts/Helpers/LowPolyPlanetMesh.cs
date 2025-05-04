using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class LowPolyPlanetMesh : MeshInstance2D
{
    [Export] public float Radius = 0.5f;
    [Export] public int Subdivisions = 2;
    [Export] public Color DefaultColor = new Color(1.0f, 1f, 1f); // Default color for triangles
    [Export] public int ContinentSeedCount = 5; //properties for continent generation
    [Export] public float ContinentNoiseScale = 1.0f;
    [Export] public float VariationFrequency = 0.1f; // How many triangles to affect (0.0-1.0)
    [Export] public float VariationIntensity = 0.2f; // How much lighter to make affected triangles (0.0-1.0)

    private List<Color> triangleColors = new List<Color>(); // Store colors for each triangle
    private bool useVertexColors = false;

    //triangle adjacency tracking
    private Dictionary<int, List<int>> triangleAdjacency = new Dictionary<int, List<int>>();
    private List<Vector3> triangleCenters = new List<Vector3>();

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

            // add three vertices for this triangle (duplicating them to avoid color interpolation)
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

            //add indices for this triangle
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

    // set triangle colors
    public void SetTriangleColors(List<Color> colors)
    {
        triangleColors = colors;
        useVertexColors = true;
        GeneratePlanetMesh(); // Rebuild the mesh with the new colors
    }

    // set a specific triangle's color
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

    // randomly color triangles (for testing)
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

        //add the new vertex
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

    // Apply properties to the planet mesh
    public void ApplyPlanetProperties(PlanetProperties properties)
    {
        if (properties.IsGasGiant)
        {
            // Use gas giant generation
            GenerateGasGiant(properties);
        }
        else
        {
            // Existing code for terrestrial planets
            if (properties.HasWater && properties.WaterAmount > 0)
            {
                // Use the existing continent generation logic
                GenerateContinents(properties);
            }
            else
            {
                // For planets without water, still color variations
                int triangleCount = GetTriangleCount();
                List<Color> colors = new List<Color>(new Color[triangleCount]);

                // Get color from properties or use default
                Color planetColor = properties.ColorIndex;
                if (planetColor.R == 0 && planetColor.G == 0 && planetColor.B == 0)
                {
                    planetColor = DefaultColor;
                }

                // Fill all triangles with the base color
                for (int i = 0; i < triangleCount; i++)
                {
                    colors[i] = planetColor;
                }

                // Apply random variations to create surface details
                AddRandomColorVariation(colors, false);  // false = don't try to preserve water/land distinction

                // Set the colors and generate mesh
                SetTriangleColors(colors);
            }
        }
    }

    // Generate continents based on the planet properties
    public void GenerateContinents(PlanetProperties properties)
    {
        int triangleCount = GetTriangleCount();
        List<Color> colors = new List<Color>(new Color[triangleCount]);

        // Define colors for land and water
        Color landColor;
        if (properties.ColorIndex.R == 0 && properties.ColorIndex.G == 0 && properties.ColorIndex.B == 0)
        {
            // If no color is specified in properties, use a default tan/brown
            landColor = new Color(0.7f, 0.5f, 0.3f); // Darker tan/brown land
        }
        else
        {
            // Use the specified color but ensure it's not too bright
            landColor = properties.ColorIndex;

            // Make sure the land color isn't too bright to begin with
            if (landColor.R > 0.8f && landColor.G > 0.8f)
            {
                landColor = new Color(
                    landColor.R * 0.7f,
                    landColor.G * 0.7f,
                    landColor.B * 0.7f
                );
            }
        }

        Color waterColor = new Color(0.2f, 0.4f, 0.8f); // Blue water

        // For high water amounts, we need to start with all water and generate land
        bool highWaterContent = properties.WaterAmount > 0.7f;

        // Initialize all triangles to the dominant terrain type
        for (int i = 0; i < triangleCount; i++)
        {
            colors[i] = highWaterContent ? waterColor : landColor;
        }

        // Calculate triangle centers and build adjacency map
        CalculateTriangleCenters();
        BuildTriangleAdjacency();

        Random random = new Random();
        int seedCount;
        HashSet<int> minorityTriangles = new HashSet<int>();

        if (highWaterContent)
        {
            // For high water: we generate LAND seeds instead
            // More water means fewer land seeds
            float landAmount = 1.0f - properties.WaterAmount;
            seedCount = Math.Max(1, (int)(ContinentSeedCount * landAmount * 3)); // Multiply by 3 to ensure enough seeds for small land

            // Place land seeds
            for (int i = 0; i < seedCount; i++)
            {
                int seed = random.Next(triangleCount);
                minorityTriangles.Add(seed);
                colors[seed] = landColor;
            }
        }
        else
        {
            // Original ocean seed generation for low water amounts
            seedCount = Math.Max(1, (int)(ContinentSeedCount * properties.WaterAmount));

            // Place ocean seeds
            for (int i = 0; i < seedCount; i++)
            {
                int seed = random.Next(triangleCount);
                minorityTriangles.Add(seed);
                colors[seed] = waterColor;
            }
        }

        // Use noise for more natural shapes
        var noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.SimplexSmooth);
        noise.SetSeed(random.Next());
        noise.SetFrequency(0.8f * ContinentNoiseScale);

        // The minority coverage is either water amount or land amount depending on which is smaller
        float targetMinorityCoverage = highWaterContent ? (1.0f - properties.WaterAmount) : properties.WaterAmount;
        float currentMinorityCoverage = (float)minorityTriangles.Count / triangleCount;

        // Expansion using breadth-first growth
        Queue<int> expansionFrontier = new Queue<int>(minorityTriangles);
        List<int> newExpansions = new List<int>();

        // Expansion iterations - grow land/ocean regions
        while (currentMinorityCoverage < targetMinorityCoverage && expansionFrontier.Count > 0)
        {
            newExpansions.Clear();

            int frontierSize = expansionFrontier.Count;
            for (int i = 0; i < frontierSize; i++)
            {
                int triangleId = expansionFrontier.Dequeue();

                // Try to expand to neighbors
                if (triangleAdjacency.TryGetValue(triangleId, out var neighbors))
                {
                    foreach (var neighbor in neighbors)
                    {
                        // Skip if already part of the minority terrain
                        if (minorityTriangles.Contains(neighbor))
                            continue;

                        // Use noise to determine expansion probability
                        Vector3 pos = triangleCenters[neighbor];
                        float noiseValue = noise.GetNoise3d(pos.X, pos.Y, pos.Z);

                        // Higher noise value = higher chance to expand
                        float expansionProbability = (noiseValue + 1f) / 2.0f;

                        if (random.NextDouble() < expansionProbability)
                        {
                            minorityTriangles.Add(neighbor);
                            colors[neighbor] = highWaterContent ? landColor : waterColor;
                            newExpansions.Add(neighbor);

                            currentMinorityCoverage = (float)minorityTriangles.Count / triangleCount;
                            if (currentMinorityCoverage >= targetMinorityCoverage)
                                break;
                        }
                    }
                }

                if (currentMinorityCoverage >= targetMinorityCoverage)
                    break;
            }

            //new expansions to frontier for next iteration
            foreach (var newFrontier in newExpansions)
                expansionFrontier.Enqueue(newFrontier);
        }

        //random color variation
        AddRandomColorVariation(colors);

        // Adjust water coverage to match target
        AdjustWaterCoverage(colors, properties.WaterAmount);

        // Apply the colors
        SetTriangleColors(colors);
    }

    // Build adjacency map between triangles
    private void BuildTriangleAdjacency()
    {
        // Skip if already built
        if (triangleAdjacency.Count > 0)
            return;

        // We'll need to recreate the full mesh to track adjacency
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        // Create the icosahedron
        GenerateIcosahedron(vertices, indices);

        // Build the mesh with subdivisions, tracking adjacency
        Dictionary<long, int> edgeToTriangles = new Dictionary<long, int>();

        // Process original icosahedron
        BuildAdjacencyForIndices(indices, edgeToTriangles);

        // Perform subdivisions
        for (int i = 0; i < Subdivisions; i++)
        {
            SubdivideIcosahedron(vertices, indices);
            // Clear and rebuild adjacency for the subdivided mesh
            triangleAdjacency.Clear();
            edgeToTriangles.Clear();
            BuildAdjacencyForIndices(indices, edgeToTriangles);
        }
    }

    // Calculate centers for all triangles
    private void CalculateTriangleCenters()
    {
        if (triangleCenters.Count > 0)
            return;

        // Get the mesh data
        if (!(Mesh is ArrayMesh arrayMesh) || arrayMesh.GetSurfaceCount() == 0)
        {
            // If no mesh yet, generate one temporarily
            GeneratePlanetMesh();
            arrayMesh = (ArrayMesh)Mesh;
        }

        Godot.Collections.Array arrays = arrayMesh.SurfaceGetArrays(0);
        Vector3[] verts = (Vector3[])arrays[(int)Mesh.ArrayType.Vertex];
        int[] indices = (int[])arrays[(int)Mesh.ArrayType.Index];

        // Calculate centers for all triangles (using the center of each three vertices)
        triangleCenters.Clear();
        for (int i = 0; i < indices.Length; i += 3)
        {
            Vector3 v1 = verts[indices[i]];
            Vector3 v2 = verts[indices[i + 1]];
            Vector3 v3 = verts[indices[i + 2]];

            // Calculate triangle center
            Vector3 center = (v1 + v2 + v3) / 3.0f;
            triangleCenters.Add(center.Normalized());
        }
    }

    // Helper to build adjacency from a list of triangle indices
    private void BuildAdjacencyForIndices(List<int> indices, Dictionary<long, int> edgeToTriangles)
    {
        // Process each triangle
        for (int i = 0; i < indices.Count; i += 3)
        {
            int triangleIndex = i / 3;

            // Make sure we have an entry for this triangle
            if (!triangleAdjacency.ContainsKey(triangleIndex))
                triangleAdjacency[triangleIndex] = new List<int>();

            // Process each edge
            for (int e = 0; e < 3; e++)
            {
                int v1 = indices[i + e];
                int v2 = indices[i + (e + 1) % 3];

                // Create consistent edge key (smaller vertex index first)
                long edgeKey = GetEdgeKey(v1, v2);

                // If this edge was seen before, it means two triangles share it
                if (edgeToTriangles.TryGetValue(edgeKey, out int otherTriangle))
                {
                    //to each other's adjacency lists
                    triangleAdjacency[triangleIndex].Add(otherTriangle);
                    triangleAdjacency[otherTriangle].Add(triangleIndex);

                    // Remove edge since we've processed both triangles using it
                    edgeToTriangles.Remove(edgeKey);
                }
                else
                {
                    // First occurrence of this edge
                    edgeToTriangles[edgeKey] = triangleIndex;
                }
            }
        }
    }

    // Helper to create a unique key for an edge
    private long GetEdgeKey(int v1, int v2)
    {
        // Always use smaller index first to ensure consistent keys
        if (v1 > v2)
        {
            int temp = v1;
            v1 = v2;
            v2 = temp;
        }

        return ((long)v1 << 32) | (uint)v2;
    }

    // Add random color variation to triangles
    public void AddRandomColorVariation(List<Color> colors, bool preserveWaterLand = true)
    {
        if (colors == null || colors.Count == 0)
            return;

        Random random = new Random();
        int triangleCount = colors.Count;

        // Determine how many triangles to affect
        int variationCount = (int)(triangleCount * VariationFrequency);

        // Create a separate list of water and land triangles
        List<int> landTriangles = new List<int>();
        List<int> waterTriangles = new List<int>();

        if (preserveWaterLand)
        {
            // Use a more reliable method to detect water vs. land
            // Water is typically more blue than red/green
            for (int i = 0; i < colors.Count; i++)
            {
                Color c = colors[i];
                // Better water detection - blue should be significantly higher than red/green
                if (c.B > 0.3f && c.B > c.R * 1.5f && c.B > c.G * 1.2f)
                {
                    waterTriangles.Add(i);
                }
                else
                {
                    landTriangles.Add(i);
                }
            }

        }

        // Function to lighten a color - enhanced version
        Color LightenColor(Color original, bool isLand)
        {
            // Apply stronger brightening to land to make it more noticeable
            float intensityMultiplier = isLand ? 1.5f : 1.0f;
            float factor = 1.0f + (VariationIntensity * intensityMultiplier * (float)random.NextDouble());

            // Create a lighter version of the color
            return new Color(
                Mathf.Clamp(original.R * factor, 0f, 1f),
                Mathf.Clamp(original.G * factor, 0f, 1f),
                Mathf.Clamp(original.B * factor, 0f, 1f)
            );
        }

        //variation while preserving land/water distinction
        if (preserveWaterLand && waterTriangles.Count > 0 && landTriangles.Count > 0)
        {
            // Increase variation frequency for land to make it more obvious
            float landVariationMultiplier = 1.5f;

            // How many triangles to affect in each category
            int waterVariations = Math.Min((int)(waterTriangles.Count * VariationFrequency), waterTriangles.Count);
            int landVariations = Math.Min((int)(landTriangles.Count * VariationFrequency * landVariationMultiplier), landTriangles.Count);

            // Shuffle the lists for random selection
            waterTriangles = waterTriangles.OrderBy(x => random.Next()).ToList();
            landTriangles = landTriangles.OrderBy(x => random.Next()).ToList();

            // Apply variation to selected water triangles
            for (int i = 0; i < waterVariations; i++)
            {
                int index = waterTriangles[i];
                colors[index] = LightenColor(colors[index], false);
            }

            // Apply variation to selected land triangles
            for (int i = 0; i < landVariations; i++)
            {
                int index = landTriangles[i];
                colors[index] = LightenColor(colors[index], true);
            }
        }
        else
        {
            // Simple random variation without preserving water/land
            for (int i = 0; i < variationCount; i++)
            {
                int index = random.Next(triangleCount);
                colors[index] = LightenColor(colors[index], false);
            }
        }
    }

    // Calculate water coverage based on triangle colors
    private float CalculateWaterCoverage(List<Color> colors)
    {
        if (colors == null || colors.Count == 0) return 0f;

        int waterTriangles = 0;

        // Count triangles that appear to be water (blue channel dominant)
        for (int i = 0; i < colors.Count; i++)
        {
            Color c = colors[i];
            if (c.B > 0.3f && c.B > c.R * 1.5f && c.B > c.G * 1.2f)
            {
                waterTriangles++;
            }
        }

        return (float)waterTriangles / colors.Count;
    }

    // Adjust water coverage to match target amount
    private void AdjustWaterCoverage(List<Color> colors, float targetWaterAmount)
    {
        // Calculate current water coverage
        float currentWaterCoverage = CalculateWaterCoverage(colors);

        // Define acceptable range (±10%)
        float minAcceptable = Math.Max(0.0f, targetWaterAmount - 0.1f);
        float maxAcceptable = Math.Min(1.0f, targetWaterAmount + 0.1f);

        // Check if current coverage is outside acceptable range
        if (currentWaterCoverage < minAcceptable || currentWaterCoverage > maxAcceptable)
        {
            Color waterColor = new Color(0.2f, 0.4f, 0.8f);
            Color landColor = new Color(0.7f, 0.5f, 0.3f);
            Random random = new Random();

            // Use breadth-first approach to expand water/land from coastal boundaries
            HashSet<int> processed = new HashSet<int>();
            Queue<int> frontier = new Queue<int>();

            // Identify all coastal triangles to start with
            for (int i = 0; i < colors.Count; i++)
            {
                bool isWater = IsWaterTriangle(colors[i]);

                if (triangleAdjacency.TryGetValue(i, out var neighbors))
                {
                    foreach (var neighbor in neighbors)
                    {
                        if (neighbor >= colors.Count) continue;

                        bool neighborIsWater = IsWaterTriangle(colors[neighbor]);

                        // If this triangle and its neighbor are different terrain types
                        if (isWater != neighborIsWater)
                        {
                            // We need more water
                            if (currentWaterCoverage < minAcceptable && !isWater)
                            {
                                frontier.Enqueue(i); //this land triangle bordering water
                                break;
                            }
                            // We need more land
                            else if (currentWaterCoverage > maxAcceptable && isWater)
                            {
                                frontier.Enqueue(i); //this water triangle bordering land
                                break;
                            }
                        }
                    }
                }
            }

            // How many triangles to convert
            int trianglesToConvert;
            if (currentWaterCoverage < minAcceptable)
            {
                trianglesToConvert = (int)((minAcceptable - currentWaterCoverage) * colors.Count);
            }
            else
            {
                trianglesToConvert = (int)((currentWaterCoverage - maxAcceptable) * colors.Count);
            }

            // Convert triangles using BFS
            int converted = 0;

            while (frontier.Count > 0 && converted < trianglesToConvert)
            {
                int triangleIdx = frontier.Dequeue();

                if (processed.Contains(triangleIdx))
                    continue;

                processed.Add(triangleIdx);

                bool isWater = IsWaterTriangle(colors[triangleIdx]);

                // Only convert triangles of the right type (land if we need more water, water if we need more land)
                if ((currentWaterCoverage < minAcceptable && !isWater) ||
                    (currentWaterCoverage > maxAcceptable && isWater))
                {
                    // Convert this triangle
                    colors[triangleIdx] = isWater ? landColor : waterColor;
                    converted++;

                    //neighbors to frontier
                    if (triangleAdjacency.TryGetValue(triangleIdx, out var neighbors))
                    {
                        foreach (var neighbor in neighbors)
                        {
                            if (neighbor >= colors.Count || processed.Contains(neighbor))
                                continue;

                            bool neighborIsWater = IsWaterTriangle(colors[neighbor]);

                            // Onlyneighbors of the same type we're converting
                            if ((currentWaterCoverage < minAcceptable && !neighborIsWater) ||
                                (currentWaterCoverage > maxAcceptable && neighborIsWater))
                            {
                                frontier.Enqueue(neighbor);
                            }
                        }
                    }
                }
            }

            // Final check
            float newCoverage = CalculateWaterCoverage(colors);
            GD.Print($"Final water coverage: {newCoverage:F2} (converted {converted} triangles)");
        }
    }

    // Helper method to check if a triangle is water
    private bool IsWaterTriangle(Color color)
    {
        return color.B > 0.3f && color.B > color.R * 1.5f && color.B > color.G * 1.2f;
    }

    public void GenerateGasGiant(PlanetProperties properties)
    {
        int triangleCount = GetTriangleCount();
        List<Color> colors = new List<Color>(new Color[triangleCount]);

        // Base planet color
        Color baseColor = properties.ColorIndex.R == 0 && properties.ColorIndex.G == 0 &&
                          properties.ColorIndex.B == 0 ?
                          new Color(0.8f, 0.7f, 0.4f) : // Default Jupiter-like color
                          properties.ColorIndex;

        // Calculate triangle centers if not already done
        CalculateTriangleCenters();

        // Create horizontal bands based on Y coordinate (latitude)
        var noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.SimplexSmooth);
        noise.SetSeed(new Random().Next());
        noise.SetFrequency(0.5f);

        for (int i = 0; i < triangleCount; i++)
        {
            // Use the Y coordinate to create horizontal bands
            Vector3 position = triangleCenters[i];

            // Latitude is based on Y coordinate (-1 to 1)
            float latitude = position.Y;

            // Create banding effect with noise - using absolute value to avoid darkening
            float bandNoise = noise.GetNoise3d(position.X * 2, position.Y * 10, position.Z * 2);

            // Use absolute value of sin to avoid negative values that create dark bands
            float bandValue = Mathf.Abs(Mathf.Sin(latitude * 15)) * 0.5f;

            // Mix noise and banding, keeping values positive
            float colorVariation = bandValue + Mathf.Abs(bandNoise * 0.3f);

            // Create lighter bands only
            Color bandColor = new Color(
                Mathf.Clamp(baseColor.R + colorVariation * 0.4f, 0.6f, 1.0f),
                Mathf.Clamp(baseColor.G + colorVariation * 0.4f, 0.6f, 1.0f),
                Mathf.Clamp(baseColor.B + colorVariation * 0.3f, 0.5f, 1.0f)
            );

            colors[i] = bandColor;
        }

        // Add storm features
        AddStormFeatures(colors, 0.2f); // 0.2 = storm density

        SetTriangleColors(colors);
    }

    private void AddStormFeatures(List<Color> colors, float stormDensity)
    {
        Random random = new Random();
        int triangleCount = colors.Count;
        int stormCount = (int)(triangleCount * stormDensity * 0.01f); // Limit storm count

        // Need adjacency for creating storm spots
        BuildTriangleAdjacency();

        // Create several storm systems
        for (int i = 0; i < stormCount; i++)
        {
            // Pick a random triangle as storm center
            int stormCenter = random.Next(triangleCount);

            // Determine storm size (1-5)
            int stormSize = random.Next(1, 6);

            // Determine storm color (only bright options now)
            Color stormColor;
            float stormType = (float)random.NextDouble();
            if (stormType < 0.4f)
                stormColor = new Color(0.9f, 0.5f, 0.3f); // Brighter orange-red
            else
                stormColor = new Color(0.95f, 0.95f, 0.8f); // Whitish/cream

            // Color the storm center and expand outward
            HashSet<int> stormTriangles = new HashSet<int>();
            Queue<int> frontier = new Queue<int>();
            frontier.Enqueue(stormCenter);

            while (frontier.Count > 0 && stormTriangles.Count < stormSize * 5)
            {
                int current = frontier.Dequeue();
                if (stormTriangles.Contains(current)) continue;

                // Add to storm
                stormTriangles.Add(current);
                colors[current] = stormColor;

                // Add neighbors to frontier for expansion
                if (triangleAdjacency.TryGetValue(current, out var neighbors))
                {
                    foreach (var neighbor in neighbors)
                    {
                        if (neighbor < triangleCount && !stormTriangles.Contains(neighbor))
                        {
                            // Fade color as we get further from center
                            float distFactor = 1.0f - ((float)stormTriangles.Count / (stormSize * 8));
                            if (distFactor > 0)
                                frontier.Enqueue(neighbor);
                        }
                    }
                }
            }
        }
    }
}
public class FastNoiseLite
{
    public enum NoiseType { SimplexSmooth, Perlin, Cellular }

    private Random random;
    private int seed;
    private float frequency = 0.01f;
    private NoiseType noiseType = NoiseType.SimplexSmooth;

    public void SetSeed(int seed)
    {
        this.seed = seed;
        this.random = new Random(seed);
    }

    public void SetFrequency(float frequency)
    {
        this.frequency = frequency;
    }

    public void SetNoiseType(NoiseType type)
    {
        this.noiseType = type;
    }

    public float GetNoise3d(float x, float y, float z)
    {
        float scale = 1.0f / frequency;
        return Mathf.Sin(x * scale * 1.7f + seed) *
               Mathf.Cos(y * scale * 1.4f + seed) *
               Mathf.Sin(z * scale * 1.9f + seed);
    }
}