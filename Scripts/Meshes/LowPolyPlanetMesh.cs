using System;
using System.Collections.Generic;
using Godot;
using System.Linq;

public partial class LowPolyPlanetMesh : LowPolyMesh
{
    #region Planet Properties
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

    #endregion
    #region  Water Coverage
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
        }
    }

    // Helper method to check if a triangle is water
    private bool IsWaterTriangle(Color color)
    {
        return color.B > 0.3f && color.B > color.R * 1.5f && color.B > color.G * 1.2f;
    }
    #endregion
    #region Gas Giant Generation
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
    #endregion
}