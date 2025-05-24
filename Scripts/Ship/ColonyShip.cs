using Godot;
using System;

public partial class ColonyShip : PlayerShips
{
    public Planet planet;
    public ColonyShip(Planet planet)
    {
        this.planet = planet;
        GD.Print("Colony ship created for planet: " + planet.Name);
    }
    public override void _Ready()
    {
        base._Ready();
        speed = 175; // Set the speed of the colony ship
        DisableMovement = true; // Disable movement for the colony ship

        Mesh = new ColonyShipMesh();
        Mesh.Scale = 10; // Set the scale of the ship
        Mesh.Rotation = -(Mathf.Pi / 2); // Rotate the ship to face upwards
        AddChild(Mesh);
        trailEffect.SetTrailWidth(3);
        area2D.AreaEntered += OnAreaEntered;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        path.Clear();
        path.Add(planet.GlobalPosition);
        // Additional processing for the colony ship
    }
    private void OnAreaEntered(Area2D area)
    {
        GD.Print("Area entered: " + area.Name);
        GD.Print(area.GetParent());
        if (area.GetParent()?.GetParent() is Planet planet)
        {
            if (planet == this.planet)
            {
                GD.Print("Entered area of planet: " + planet.Name);
                Colony colony = new Colony(planet);
                planet.AddChild(colony);
                planet.hasColony = true;
                ColonyUI colonyUI = new ColonyUI(colony);
                planet.AddChild(colonyUI);
                planet.colonyUI = colonyUI;
                QueueFree();
                // Handle the interaction with the planet here
            }
        }
    }
}
