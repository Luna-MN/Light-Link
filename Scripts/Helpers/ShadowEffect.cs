using Godot;
using System.Collections.Generic;

public partial class ShadowEffect : Node2D
{
    [Export]
    private float shadowIntensity = 0.7f;

    [Export]
    private float shadowLength = 400.0f;

    [Export]
    private Color shadowColor = new Color(0, 0, 0, 0.5f);

    [Export]
    private bool useGradient = true;

    public Star star;

    // Objects that will cast shadows
    private List<Planet> Planets = new List<Planet>();

    public override void _Ready()
    {
        ZIndex = 10; // Ensure this is drawn above the planets
    }

    public override void _Process(double delta)
    {
        if (star != null)
        {
            QueueRedraw();
        }
    }

    public void AddOccluder(Planet occluder)
    {
        if (!Planets.Contains(occluder))
        {
            Planets.Add(occluder);
        }
    }

    public override void _Draw()
    {
        if (star == null || !star.Visible || star.zoomedOut)
        {
            GD.Print("Star is null or not visible or zoomed out.");
            Visible = false;
            return;
        }
        else
        {
            Visible = true;
        }

        foreach (var planet in Planets)
        {
            DrawShadowForObject(planet);
        }
    }

    private void DrawShadowForObject(Planet planet)
    {
        // Calculate direction from occluder to star (this is the opposite of your current direction)
        Vector2 dirToLight = (star.GlobalPosition - planet.GlobalPosition).Normalized();
        Vector2 shadowDir = -dirToLight; // The shadow points away from the light source

        // Get the actual size from the occluder if possible
        float radius = planet.Properties.Radius * 0.5f;

        // Calculate the position in global coordinates
        Vector2 occluderPos = ToLocal(planet.GlobalPosition);

        // Calculate shadow polygon points
        Vector2[] points = CalculateShadowPolygon(occluderPos, radius, shadowDir);

        // Draw the shadow
        if (useGradient)
        {
            Color startColor = shadowColor;
            Color endColor = new Color(shadowColor.R, shadowColor.G, shadowColor.B, 0);

            for (int i = 0; i < 10; i++)
            {
                float t = i / 10.0f;
                float len = Mathf.Lerp(0, shadowLength, t);
                Color color = startColor.Lerp(endColor, t);

                Vector2[] segmentPoints = {
                points[0] + shadowDir * len,
                points[1] + shadowDir * len,
                points[1] + shadowDir * (len + shadowLength/10),
                points[0] + shadowDir * (len + shadowLength/10)
            };

                DrawColoredPolygon(segmentPoints, color);
            }
        }
        else
        {
            DrawColoredPolygon(points, shadowColor);
        }
    }

    private Vector2[] CalculateShadowPolygon(Vector2 center, float radius, Vector2 shadowDir)
    {
        // Create perpendicular vector to get the sides of the shadow
        Vector2 perpendicular = new Vector2(-shadowDir.Y, shadowDir.X).Normalized() * radius;

        // Calculate the four corners of the shadow polygon
        Vector2[] points = new Vector2[4];
        points[0] = center + perpendicular;
        points[1] = center - perpendicular;
        points[2] = points[1] + shadowDir * shadowLength;
        points[3] = points[0] + shadowDir * shadowLength;

        return points;
    }
}