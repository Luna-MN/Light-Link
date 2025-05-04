using System;
using Godot;
using System.Collections.Generic;

public partial class LowPolyStarMesh : LowPolyMesh
{

    #region Star Generation

    public void GenerateStar(float temperature = 5800f, float activityLevel = 0.5f)
    {
        int triangleCount = GetTriangleCount();
        List<Color> colors = new List<Color>(new Color[triangleCount]);

        // Determine base color based on temperature
        Color baseColor = DetermineStarColor(temperature);

        // Calculate triangle centers if not already done
        CalculateTriangleCenters();
        BuildTriangleAdjacency();

        // Generate surface turbulence and features
        var noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.SimplexSmooth);
        noise.SetSeed(new Random().Next());
        noise.SetFrequency(0.6f);

        for (int i = 0; i < triangleCount; i++)
        {
            Vector3 position = triangleCenters[i];

            // Generate turbulent plasma surface
            float turbulence = noise.GetNoise3d(position.X * 3, position.Y * 3, position.Z * 3);

            // Add surface features (brighter spots)
            float feature = Math.Max(0, noise.GetNoise3d(position.X * 5, position.Y * 5, position.Z * 5));

            // Combine effects
            float brightness = 1.0f + turbulence * 0.2f + feature * 0.3f;

            // Apply to base color
            colors[i] = new Color(
                Mathf.Clamp(baseColor.R * brightness, 0, 1),
                Mathf.Clamp(baseColor.G * brightness, 0, 1),
                Mathf.Clamp(baseColor.B * brightness, 0, 1)
            );
        }

        // Add solar flare/prominence features
        AddSolarProminences(colors, activityLevel);

        GD.Print($"Generated star with temperature: {temperature} and activity level: {activityLevel}");

        SetTriangleColors(colors);
    }

    // Fixed color mapping based on accurate stellar classification
    private Color DetermineStarColor(float temperature)
    {
        if (temperature <= 3200)
            return new Color(1.0f, 0.4f, 0.1f);  // M-type (red dwarf)
        else if (temperature <= 4000)
            return new Color(1.0f, 0.5f, 0.2f);  // K-type (orange)
        else if (temperature <= 5200)
            return new Color(1.0f, 0.7f, 0.3f);  // K-G transition (orange-yellow)
        else if (temperature <= 6000)
            return new Color(1.0f, 0.9f, 0.4f);  // G-type (yellow, Sun-like)
        else if (temperature <= 7200)
            return new Color(1.0f, 1.0f, 0.8f);  // F-type (yellow-white)
        else if (temperature <= 10000)
            return new Color(0.95f, 0.97f, 1.0f); // A-type (white)
        else if (temperature <= 30000)
            return new Color(0.8f, 0.85f, 1.0f);  // B-type (blue-white)
        else
            return new Color(0.7f, 0.7f, 1.0f);   // O-type (blue)
    }

    private void AddSolarProminences(List<Color> colors, float activityLevel)
    {
        Random random = new Random();
        int triangleCount = colors.Count;
        int prominenceCount = (int)(triangleCount * activityLevel * 0.5f);
        // Need adjacency for creating prominences
        BuildTriangleAdjacency();

        // Create several prominences
        for (int i = 0; i < prominenceCount; i++)
        {
            // Pick a random triangle as prominence center
            int prominenceCenter = random.Next(triangleCount);

            // Determine prominence size
            int prominenceSize = random.Next(2, 8);

            // Color the prominence center and expand outward
            HashSet<int> prominenceTriangles = new HashSet<int>();
            Queue<int> frontier = new Queue<int>();
            frontier.Enqueue(prominenceCenter);

            while (frontier.Count > 0 && prominenceTriangles.Count < prominenceSize * 3)
            {
                int current = frontier.Dequeue();
                if (prominenceTriangles.Contains(current)) continue;

                // Add to prominence
                prominenceTriangles.Add(current);

                // Brighten existing color rather than replacing
                Color originalColor = colors[current];
                colors[current] = new Color(
                    Mathf.Clamp(originalColor.R * 1.5f, 0, 1),
                    Mathf.Clamp(originalColor.G * 1.5f, 0, 1),
                    Mathf.Clamp(originalColor.B * 1.2f, 0, 1)
                );

                // Add neighbors to frontier for expansion
                if (triangleAdjacency.TryGetValue(current, out var neighbors))
                {
                    foreach (var neighbor in neighbors)
                    {
                        if (neighbor < triangleCount && !prominenceTriangles.Contains(neighbor))
                        {
                            float distFactor = 1.0f - ((float)prominenceTriangles.Count / (prominenceSize * 3));
                            if (distFactor > 0 && random.NextDouble() < distFactor * 0.8)
                                frontier.Enqueue(neighbor);
                        }
                    }
                }
            }
        }
    }
    # endregion

}