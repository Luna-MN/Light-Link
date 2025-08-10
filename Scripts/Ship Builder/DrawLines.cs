using Godot;
using System;
[GlobalClass]
public partial class DrawLines : Node2D
{
    [Export] public int MaxSize = 2000;
    [Export] public float VisibleRadius = 100.0f; // Radius of the visibility circle around the mouse
    [Export] public float GridSpacing = 20.0f; // Space between grid lines
    [Export] public float LineWidth = 0.25f; // Width of the grid lines
    [Export] public float FadeStartRadius = 0.1f; // Percentage of the radius where fading starts (0.0-1.0)
    
    private Vector2 _mousePosition = Vector2.Zero;
    
    public override void _Ready()
    {
        // Ensure we get redrawn every frame
        ProcessMode = ProcessModeEnum.Always;
    }
    
    public override void _Process(double delta)
    {
        // Update mouse position
        _mousePosition = GetGlobalMousePosition();
        
        // Queue redraw to update the grid
        QueueRedraw();
    }
    
    public override void _Draw()
    {
        // Calculate the visible area
        int startX = Mathf.Max(0, (int)(_mousePosition.X - VisibleRadius) / (int)GridSpacing * (int)GridSpacing);
        int endX = Mathf.Min(MaxSize, (int)(_mousePosition.X + VisibleRadius) / (int)GridSpacing * (int)GridSpacing + (int)GridSpacing);
        int startY = Mathf.Max(0, (int)(_mousePosition.Y - VisibleRadius) / (int)GridSpacing * (int)GridSpacing);
        int endY = Mathf.Min(MaxSize, (int)(_mousePosition.Y + VisibleRadius) / (int)GridSpacing * (int)GridSpacing + (int)GridSpacing);
        
        // Calculate where fading begins
        float fadeStartDistance = VisibleRadius * FadeStartRadius;
        
        // Draw vertical lines
        for (int x = startX; x <= endX; x += (int)GridSpacing)
        {
            // Skip lines outside the visible radius
            if (x < _mousePosition.X - VisibleRadius || x > _mousePosition.X + VisibleRadius)
                continue;
                
            for (int y = startY; y <= endY; y += (int)GridSpacing)
            {
                // For each grid cell, check if any part of the line is within the visible circle
                // Calculate start and end points for the line segment
                Vector2 lineStart = new Vector2(x, y);
                Vector2 lineEnd = new Vector2(x, y + GridSpacing);
                
                // Check if either endpoint is within the circle or if the line crosses the circle
                if (IsPointInCircle(lineStart) || IsPointInCircle(lineEnd) || 
                    DoesLineIntersectCircle(lineStart, lineEnd))
                {
                    // Calculate opacity based on distance from mouse position
                    float opacity = CalculateLineOpacity(lineStart, lineEnd, fadeStartDistance);
                    
                    // Create color with calculated opacity
                    Color lineColor = new Color(1, 1, 1, opacity);
                    
                    // Draw the line segment with the calculated opacity
                    DrawLine(lineStart, lineEnd, lineColor, LineWidth);
                }
            }
        }
        
        // Draw horizontal lines
        for (int y = startY; y <= endY; y += (int)GridSpacing)
        {
            // Skip lines outside the visible radius
            if (y < _mousePosition.Y - VisibleRadius || y > _mousePosition.Y + VisibleRadius)
                continue;
                
            for (int x = startX; x <= endX; x += (int)GridSpacing)
            {
                // For each grid cell, check if any part of the line is within the visible circle
                // Calculate start and end points for the line segment
                Vector2 lineStart = new Vector2(x, y);
                Vector2 lineEnd = new Vector2(x + GridSpacing, y);
                
                // Check if either endpoint is within the circle or if the line crosses the circle
                if (IsPointInCircle(lineStart) || IsPointInCircle(lineEnd) || 
                    DoesLineIntersectCircle(lineStart, lineEnd))
                {
                    // Calculate opacity based on distance from mouse position
                    float opacity = CalculateLineOpacity(lineStart, lineEnd, fadeStartDistance);
                    
                    // Create color with calculated opacity
                    Color lineColor = new Color(1, 1, 1, opacity);
                    
                    // Draw the line segment with the calculated opacity
                    DrawLine(lineStart, lineEnd, lineColor, LineWidth);
                }
            }
        }
    }
    
    // Calculate opacity based on line distance from center
    private float CalculateLineOpacity(Vector2 start, Vector2 end, float fadeStartDistance)
    {
        // Find the midpoint of the line segment
        Vector2 midpoint = (start + end) / 2;
        
        // Calculate distance from mouse to midpoint
        float distance = midpoint.DistanceTo(_mousePosition);
        
        // If inside the fade start radius, full opacity
        if (distance <= fadeStartDistance)
            return 1.0f;
            
        // If outside visible radius, zero opacity (shouldn't happen due to earlier checks)
        if (distance >= VisibleRadius)
            return 0.0f;
            
        // Linear interpolation of opacity from 1.0 at fadeStartDistance to 0.0 at VisibleRadius
        return 1.0f - (distance - fadeStartDistance) / (VisibleRadius - fadeStartDistance);
    }
    
    // Check if a point is inside the visibility circle
    private bool IsPointInCircle(Vector2 point)
    {
        return point.DistanceTo(_mousePosition) <= VisibleRadius;
    }
    
    // Check if a line segment intersects with the visibility circle
    private bool DoesLineIntersectCircle(Vector2 lineStart, Vector2 lineEnd)
    {
        // Vector from start to mouse
        Vector2 d = lineEnd - lineStart;
        Vector2 f = lineStart - _mousePosition;
        
        // Quadratic equation coefficients
        float a = d.Dot(d);
        float b = 2 * f.Dot(d);
        float c = f.Dot(f) - VisibleRadius * VisibleRadius;
        
        float discriminant = b * b - 4 * a * c;
        
        if (discriminant < 0)
        {
            // No intersection
            return false;
        }
        else
        {
            // Calculate intersection points
            discriminant = Mathf.Sqrt(discriminant);
            float t1 = (-b - discriminant) / (2 * a);
            float t2 = (-b + discriminant) / (2 * a);
            
            // Check if intersection is within line segment
            return (t1 >= 0 && t1 <= 1) || (t2 >= 0 && t2 <= 1);
        }
    }
}