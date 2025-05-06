using Godot;
using System;
using System.Collections.Generic;

public partial class PlanetContextMenu : ContextMenu
{
    public Planet planet;
    public Camera camera;
    // Called when the node enters the scene tree for the first time.
    public PlanetContextMenu(Planet planet)
    {
        this.planet = planet;
    }
    public override void _Ready()
    {
        base._Ready();
        camera = (Camera)GetViewport().GetCamera2D();
        AddItem("View Planet", OnViewPlanet);
        AddItem("Scan Planet", OnScanPlanet);
        AddItem("Set Up Colony", SetUpColony);
    }

    private void OnViewPlanet()
    {
        // Logic to view the planet
        camera.targetBody = planet;
        planet.planetUI.SetUIVisible(true);
        GD.Print("Viewing planet...");
    }

    private void OnScanPlanet()
    {
        // Logic to scan the planet
        GD.Print("Scanning planet...");
    }

    private void SetUpColony()
    {
        // Logic to land on the planet
        GD.Print("Landing on planet...");
    }
}
