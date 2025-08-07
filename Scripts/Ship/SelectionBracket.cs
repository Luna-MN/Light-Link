using Godot;
using System;

public partial class SelectionBracket : Node2D
{
    [Export] public float BracketLength = 16.0f;
    [Export] public float BracketThickness = 3.0f;
    [Export] public Color BracketColor = Colors.White;

    private Rect2 bounds;

    // Call this to update the bracket position/size
    public void CreateSelectionBrackets(Rect2 newBounds)
    {
        bounds = newBounds;
    }

    public override void _Draw()
    {
        // Draw corner brackets
        // Top left
        DrawLine(bounds.Position, bounds.Position + new Vector2(BracketLength, 0), BracketColor, BracketThickness);
        DrawLine(bounds.Position, bounds.Position + new Vector2(0, BracketLength), BracketColor, BracketThickness);
        // Top right
        Vector2 topRight = bounds.Position + new Vector2(bounds.Size.X, 0);
        DrawLine(topRight, topRight - new Vector2(BracketLength, 0), BracketColor, BracketThickness);
        DrawLine(topRight, topRight + new Vector2(0, BracketLength), BracketColor, BracketThickness);
        // Bottom left
        Vector2 bottomLeft = bounds.Position + new Vector2(0, bounds.Size.Y);
        DrawLine(bottomLeft, bottomLeft + new Vector2(BracketLength, 0), BracketColor, BracketThickness);
        DrawLine(bottomLeft, bottomLeft - new Vector2(0, BracketLength), BracketColor, BracketThickness);
        // Bottom right
        Vector2 bottomRight = bounds.Position + bounds.Size;
        DrawLine(bottomRight, bottomRight - new Vector2(BracketLength, 0), BracketColor, BracketThickness);
        DrawLine(bottomRight, bottomRight - new Vector2(0, BracketLength), BracketColor, BracketThickness);
    }
}