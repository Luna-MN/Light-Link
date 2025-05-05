using Godot;
using System;

public partial class StarUI : CelestialUI
{
    // UI elements

    private Label starNameLabel;
    private Label starTypeLabel;
    private Label starPropertiesLabel;

    // Current star properties
    private StarProperties currentProperties;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // You might want to update the UI position if the star moves
        if (currentProperties != null)
        {
            UpdateUIPosition();
        }
    }

    public void SetStarProperties(StarProperties properties)
    {
        currentProperties = properties;

        starNameLabel = AddProperty("Star Name", properties.Name ?? "Unknown Star");
        starTypeLabel = AddProperty("Star Type", properties.SType);
        starPropertiesLabel = AddProperty("Star Properties",
            $"\nSize: {properties.Radius}\n" +
            $"Temperature: {properties.Temperature}K");

        // Position UI around the star
        UpdateUIPosition();
    }
    private void UpdateUIPosition()
    {
        if (currentProperties == null)
            return;

        // The star itself is at (0,0) in local coordinates
        Vector2 starPosition = Vector2.Zero;
        float starRadius = currentProperties.Radius * 1.5f; // Smaller scale factor to avoid huge brackets

        // Position the selection brackets directly around the star
        selectionBrackets.Position = Vector2.Zero;
        selectionBrackets.Scale = Vector2.One * (starRadius / 10.0f); // Adjust scale to match star size

        // Position the info panel more reasonably
        infoPanel.Position = new Vector2(
            starRadius + 20, // To the right of the star
            -50 // Above the star
        );

        // Fix the connection line
        connectionLine.ClearPoints();
        // Start at edge of the star
        connectionLine.AddPoint(new Vector2(starRadius * 0.5f, -starRadius * 0.5f));
        // Connect to left side of info panel
        connectionLine.AddPoint(infoPanel.Position + new Vector2(0, 20));
    }


}