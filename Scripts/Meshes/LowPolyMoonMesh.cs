using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class LowPolyMoonMesh : LowPolyMesh
{
    public int CraterCount = 8;
    public float CraterSize = 0.25f; // 0-1 range, relative to moon size
    public float CraterDarkness = 0.25f; // 0-1 how dark craters should be
    public float CraterFalloff = 2.5f; // Controls crater edge sharpness
    public Color BaseMoonColor = new Color(0.85f, 0.85f, 0.87f); // Light gray
    public bool AddRandomVariation = true;
    public float MoonVariationIntensity = 0.1f; // Subtle variations

    private FastNoiseLite noise;

    public override void _Ready()
    {
        // Set default properties for a moon
        DefaultColor = BaseMoonColor;
        VariationFrequency = 0.3f;
        VariationIntensity = MoonVariationIntensity;

        // Set up noise for crater placement
        noise = new FastNoiseLite();
        noise.SetSeed(Godot.Time.GetTicksMsec().GetHashCode());

        GenerateMoonMesh();
    }

    // New method to configure the moon based on MoonProperties
    public void ConfigureFromProperties(MoonProperties properties)
    {
        // Set color based on ColorIndex
        SetColorIndex(properties.ColorIndex);
    }

    // New method to set color and adjust crater characteristics based on ColorIndex
    public void SetColorIndex(Color colorIndex)
    {
        // Set base moon color based on color index
        // Start with a light gray base and tint it according to the color index
        BaseMoonColor = new Color(
            0.7f + 0.3f * colorIndex.R,
            0.7f + 0.3f * colorIndex.G,
            0.7f + 0.3f * colorIndex.B
        );

        // Modify crater characteristics based on the color components

        // Red component influences crater size
        CraterSize = 0.15f + colorIndex.R * 0.25f; // Range: 0.15-0.4

        // Green component influences crater count
        CraterCount = 5 + Mathf.RoundToInt(colorIndex.G * 15); // Range: 5-20

        // Blue component influences crater darkness
        CraterDarkness = 0.15f + colorIndex.B * 0.35f; // Range: 0.15-0.5

        // Combined color intensity influences crater falloff
        float colorIntensity = (colorIndex.R + colorIndex.G + colorIndex.B) / 3.0f;
        CraterFalloff = 1.8f + colorIntensity * 2.5f; // Range: 1.8-4.3

        // Update default color for the mesh
        DefaultColor = BaseMoonColor;

        // Regenerate the moon mesh with the new parameters
        GenerateMoonMesh();
    }

    public void GenerateMoonMesh()
    {
        // First generate the base mesh
        GeneratePlanetMesh();

        // Calculate and store triangle centers for crater placement
        CalculateTriangleCenters();

        // Generate moon surface with craters
        List<Color> moonColors = GenerateMoonColors();

        // Apply colors to the mesh
        SetTriangleColors(moonColors);
    }

    private List<Color> GenerateMoonColors()
    {
        // Create list of colors for each triangle, starting with base color
        int triangleCount = GetTriangleCount();
        List<Color> colors = new List<Color>(triangleCount);

        for (int i = 0; i < triangleCount; i++)
        {
            colors.Add(BaseMoonColor);
        }

        // Add craters
        AddCraters(colors);

        // Add subtle random variations for realism if enabled
        if (AddRandomVariation)
        {
            AddRandomColorVariation(colors, false);
        }

        return colors;
    }

    private void AddCraters(List<Color> colors)
    {
        if (triangleCenters.Count == 0 || colors.Count == 0)
            return;

        Random random = new Random();

        // Generate random crater positions
        List<Vector3> craterCenters = new List<Vector3>();
        List<float> craterRadii = new List<float>();

        for (int i = 0; i < CraterCount; i++)
        {
            // Create a random direction for the crater center
            float x = (float)(random.NextDouble() * 2.0 - 1.0);
            float y = (float)(random.NextDouble() * 2.0 - 1.0);
            float z = (float)(random.NextDouble() * 2.0 - 1.0);

            Vector3 craterCenter = new Vector3(x, y, z).Normalized();
            craterCenters.Add(craterCenter);

            // Random radius for this crater (adjusted by overall crater size)
            float radius = (float)(random.NextDouble() * 0.5 + 0.5) * CraterSize;
            craterRadii.Add(radius);
        }

        // Apply crater coloring
        for (int i = 0; i < triangleCenters.Count; i++)
        {
            Vector3 triangleCenter = triangleCenters[i];
            float totalDarkening = 0;

            // Check if this triangle is within any crater
            for (int c = 0; c < craterCenters.Count; c++)
            {
                Vector3 craterCenter = craterCenters[c];
                float radius = craterRadii[c];

                // Calculate normalized distance from triangle center to crater center
                float distance = triangleCenter.DistanceTo(craterCenter);

                // If triangle is within the crater radius
                if (distance < radius)
                {
                    // Calculate darkening based on distance from crater center (more dark in center)
                    float normalizedDistance = distance / radius;
                    float craterEffect = Mathf.Pow(1.0f - normalizedDistance, CraterFalloff);

                    // Accumulate darkening effect (allows overlapping craters)
                    totalDarkening = Mathf.Max(totalDarkening, craterEffect * CraterDarkness);
                }
            }

            // Apply darkening to the triangle color
            if (totalDarkening > 0)
            {
                colors[i] = new Color(
                    colors[i].R * (1.0f - totalDarkening),
                    colors[i].G * (1.0f - totalDarkening),
                    colors[i].B * (1.0f - totalDarkening)
                );
            }
        }
    }

    // Add some highlands/plains variation to the moon
    public void AddSurfaceFeatures()
    {
        if (triangleCenters.Count == 0)
            CalculateTriangleCenters();

        int triangleCount = GetTriangleCount();
        List<Color> currentColors = new List<Color>(triangleCount);

        // Get current colors or create new ones based on the base color
        if (triangleColors.Count == triangleCount)
        {
            currentColors.AddRange(triangleColors);
        }
        else
        {
            for (int i = 0; i < triangleCount; i++)
                currentColors.Add(BaseMoonColor);
        }

        // Set up noise for surface features
        FastNoiseLite featureNoise = new FastNoiseLite();
        featureNoise.SetSeed(Godot.Time.GetTicksMsec().GetHashCode() * 31);
        featureNoise.SetFrequency(0.5f);

        // Apply noise-based color variations
        for (int i = 0; i < triangleCenters.Count; i++)
        {
            Vector3 center = triangleCenters[i];
            float noiseValue = featureNoise.GetNoise3d(center.X, center.Y, center.Z);

            // Map noise to color variation
            float variation = (noiseValue * 0.5f + 0.5f) * 0.2f; // 0-0.2 range

            // Lighter areas for highlands, darker for plains
            if (noiseValue > 0.2f)
            {
                // Highlands - slightly lighter
                currentColors[i] = new Color(
                    Mathf.Min(currentColors[i].R * (1.0f + variation), 1.0f),
                    Mathf.Min(currentColors[i].G * (1.0f + variation), 1.0f),
                    Mathf.Min(currentColors[i].B * (1.0f + variation), 1.0f)
                );
            }
            else
            {
                // Plains - slightly darker
                currentColors[i] = new Color(
                    currentColors[i].R * (1.0f - variation * 0.5f),
                    currentColors[i].G * (1.0f - variation * 0.5f),
                    currentColors[i].B * (1.0f - variation * 0.5f)
                );
            }
        }

        // Apply colors
        SetTriangleColors(currentColors);
    }
}