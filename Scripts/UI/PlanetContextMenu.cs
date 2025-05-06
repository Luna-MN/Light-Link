using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PlanetContextMenu : ContextMenu
{
    public Planet planet;
    public Camera camera;
    public MainShip mainShip;
    // Called when the node enters the scene tree for the first time.
    public PlanetContextMenu(Planet planet)
    {
        this.planet = planet;
    }
    public override void _Ready()
    {
        base._Ready();

        camera = (Camera)GetViewport().GetCamera2D();
        mainShip = GetTree().Root.FindChild("MainShip", true, false) as MainShip;

        AddItem("View Planet", OnViewPlanet);
        AddItem("Scan Planet", OnScanPlanet);
        if (planet.Properties.Habitability > 0.3f)
        {
            AddItem("Set Up Colony", SetUpColony);
        }
        else
        {
            AddDisabledItem("Set Up Colony");
        }
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
        ColonyShip colonyShip = new ColonyShip(planet);
        colonyShip.Position = mainShip.Position;
        GetTree().Root.AddChild(colonyShip);
        // Logic to land on the planet
        GD.Print("Landing on planet...");
    }
}
