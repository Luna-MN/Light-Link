using Godot;
using System.Collections.Generic;
[GlobalClass]
public partial class TrailEffect : Line2D
{
    public int TrailLength = 10;
    public float MaxWidth = 2.0f;
    public float MinWidth = 0.5f;
    private Queue<Vector2> trailPoints = new Queue<Vector2>();
    private Node2D parentNode;
    private Vector2 lastGlobalPosition;

    public TrailEffect()
    {

    }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Width = MaxWidth;

        // Set the trail to be drawn behind the parent
        ZIndex = -1;

        // Create width curve for gradual width reduction
        Curve widthCurve = new Curve();
        widthCurve.AddPoint(new Vector2(0, MinWidth / MaxWidth)); // Oldest point (thinnest)
        widthCurve.AddPoint(new Vector2(1, 1.0f)); // Newest point (thickest)
        WidthCurve = widthCurve;

        // Find the parent node (Planet or other Body)
        parentNode = GetParent<Node2D>();
        if (parentNode == null)
        {
            GD.PrintErr("TrailEffect must be a child of a Node2D");
        }

        // Initialize position tracking
        if (parentNode != null)
        {
            lastGlobalPosition = parentNode.GlobalPosition;
        }

        // Set the global position to Vector2.Zero to properly use the parent's transformations
        GlobalPosition = Vector2.Zero;
    }

    public override void _Process(double delta)
    {
        if (parentNode == null) return;

        // Don't modify position - keep at local origin
        Position = Vector2.Zero;

        // Store global positions but convert them to local for display
        UpdateTrail();
    }
    #region updates
    private void UpdateTrail()
    {
        // Add current global position to the trail
        Vector2 currentGlobalPos = parentNode.GlobalPosition;

        trailPoints.Enqueue(currentGlobalPos);
        lastGlobalPosition = currentGlobalPos;

        // Remove oldest point if we exceed the trail length
        while (trailPoints.Count > TrailLength)
        {
            trailPoints.Dequeue();
        }

        // Convert all stored global positions to local positions for drawing
        var points = new Vector2[trailPoints.Count];
        int i = 0;
        foreach (var point in trailPoints)
        {
            points[i] = ToLocal(point);
            i++;
        }

        // Update the line points
        Points = points;

    }
    private void UpdateWidthCurve()
    {
        Width = MaxWidth;

        Curve widthCurve = new Curve();
        widthCurve.AddPoint(new Vector2(0, MinWidth / MaxWidth)); // Oldest point (thinnest)
        widthCurve.AddPoint(new Vector2(1, 1.0f)); // Newest point (thickest)
        WidthCurve = widthCurve;
    }
    #endregion
    #region setters
    public void SetTrailWidth(float width)
    {
        MaxWidth = width;
        MinWidth = width * 0.125f; // Make minimum width proportional to maximum
        UpdateWidthCurve();
    }


    public void SetTrailColor(Color color)
    {
        DefaultColor = color;
    }
    #endregion
}