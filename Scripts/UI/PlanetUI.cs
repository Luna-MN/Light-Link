using Godot;
using System;

public partial class PlanetUI : CelestialUI
{
    // UI elements

    private Label PlanetNameLabel;
    private Label planetTypeLabel;
    private Label planetPropertiesLabel;

    // Current star properties
    private PlanetProperties currentProperties;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // You might want to update the UI position if the Planet moves
        if (currentProperties != null)
        {
            UpdateUIPosition();
        }
    }

    public void SetPlanetProperties(PlanetProperties properties)
    {
        currentProperties = properties;

        PlanetNameLabel = AddProperty("Planet Name", properties.Name ?? "Unknown Planet");
        planetTypeLabel = AddProperty("Planet Type", properties.IsGasGiant ? "Gas Giant" : "Terrestrial");
        planetPropertiesLabel = AddProperty("Planet Properties",
            $"\nSize: {properties.Radius}\n" +
            $"Habitability: {Mathf.Round(properties.Habitability * 100)}%");

        // Position UI around the star
        UpdateUIPosition();
    }
    private void UpdateUIPosition()
    {
        if (currentProperties == null)
            return;

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