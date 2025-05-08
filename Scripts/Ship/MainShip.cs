using Godot;
using System;

public partial class MainShip : PlayerShips
{
    public Line2D tractorBeam;
    public Astroid mineObject;
    public float MineDistance = 1000f; // Distance to mine asteroids
    public override void _Ready()
    {
        // Initialize ship properties
        Mesh = new PlayerShipMesh();
        Mesh.Scale = 10; // Set the scale of the ship+
        AddChild(Mesh);


        base._Ready();
        trailEffect.SetTrailWidth(5);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        // Additional processing for the main ship
        if (tractorBeam != null && mineObject != null)
        {
            tractorBeam.Points = new Vector2[]
            {
                ToLocal(GlobalPosition),
                ToLocal(mineObject.GlobalPosition)
            };
        }
    }
    public async void MineAstroid(Astroid asteroid)
    {
        if (tractorBeam != null)
        {
            return;
        }

        mineObject = asteroid;

        // Create tractor beam visual
        tractorBeam = new Line2D();
        tractorBeam.Width = 2;
        tractorBeam.ZIndex = -1;
        Color color = Colors.Cyan - new Color(0, 0, 0, 0.5f);
        tractorBeam.DefaultColor = color;
        tractorBeam.Points = new Vector2[]
        {
        ToLocal(GlobalPosition),
        ToLocal(asteroid.GlobalPosition)
        };
        AddChild(tractorBeam);

        // Animate the beam or add particle effects here

        // Wait for visual effect duration
        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
        if (asteroid.GetParent()?.GetParent() is Star star)
        {
            star.Astroids.Remove(asteroid);
        }
        // Remove asteroid and tractor beam
        mineObject = null;
        asteroid.QueueFree();
        tractorBeam.QueueFree();
        tractorBeam = null;

    }
    public float GetApproachDistance()
    {
        return Mesh.Scale;
    }
}
