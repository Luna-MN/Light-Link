using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class MainShip : PlayerShips
{
    public Line2D tractorBeam;
    public Astroid mineObject;
    public float MineDistance = 1000f; // Distance to mine asteroids
    public float AutoMineDistance = 500f; // Distance to auto mine asteroids
    public List<Astroid> MiningAstroids = new List<Astroid>();
    private bool isMiningInProgress = false;

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
        if (tractorBeam != null && IsInstanceValid(mineObject))
        {
            tractorBeam.Points = new Vector2[]
            {
                ToLocal(GlobalPosition),
                ToLocal(mineObject.GlobalPosition)
            };
        }
    }

    public void startMining(Astroid asteroid)
    {
        if (asteroid == null || !IsInstanceValid(asteroid))
            return;

        if (!MiningAstroids.Contains(asteroid))
        {
            MiningAstroids.Add(asteroid);
        }

        if (!isMiningInProgress)
        {
            isMiningInProgress = true;
            _ = MineAstroids().ContinueWith(_ => isMiningInProgress = false);
        }
    }

    private async Task MineAstroids()
    {
        while (MiningAstroids.Count > 0)
        {
            Astroid asteroid = MiningAstroids[0];

            if (asteroid == null || !IsInstanceValid(asteroid))
            {
                MiningAstroids.RemoveAt(0);
                continue;
            }

            // Move towards asteroid until within mining distance
            while (IsInstanceValid(asteroid) && GlobalPosition.DistanceTo(asteroid.GlobalPosition) > AutoMineDistance)
            {
                targetPosition = asteroid.GlobalPosition;
                await ToSignal(GetTree(), "process_frame");
            }

            if (IsInstanceValid(asteroid))
            {
                await MineAstroid(asteroid);
            }

            if (MiningAstroids.Count > 0)
                MiningAstroids.RemoveAt(0);
        }
    }

    private async Task MineAstroid(Astroid asteroid)
    {
        if (tractorBeam != null || asteroid == null || !IsInstanceValid(asteroid))
        {
            return;
        }

        mineObject = asteroid;

        // Create tractor beam visual
        tractorBeam = new Line2D
        {
            Width = 2,
            ZIndex = -1,
            DefaultColor = Colors.Cyan - new Color(0, 0, 0, 0.5f),
            Points = new Vector2[]
            {
                ToLocal(GlobalPosition),
                ToLocal(asteroid.GlobalPosition)
            }
        };
        AddChild(tractorBeam);

        try
        {
            // Wait for visual effect duration
            await ToSignal(GetTree().CreateTimer(5.0f), "timeout");
            Color color;
            Star star_ = null;
            switch (asteroid.Properties.AstroidType)
            {
                case Properties.Type.Rock:
                    color = Colors.Brown;
                    break;
                case Properties.Type.Ice:
                    color = Colors.LightBlue;
                    break;
                case Properties.Type.Iron:
                    color = Colors.Gray;
                    break;
                case Properties.Type.Carbon:
                    color = Colors.DarkGray;
                    break;
                default:
                    color = Colors.White;
                    break;
            }

            if (IsInstanceValid(asteroid) && asteroid.GetParent()?.GetParent() is Star star)
            {
                CreateResources(color, star, asteroid);
                star.Astroids.Remove(asteroid);
            }
        }
        finally
        {
            // Cleanup - execute even if there was an exception
            mineObject = null;

            if (asteroid != null && IsInstanceValid(asteroid))
                asteroid.QueueFree();

            if (tractorBeam != null && IsInstanceValid(tractorBeam))
            {
                tractorBeam.QueueFree();
                tractorBeam = null;
            }
        }
    }

    public float GetApproachDistance()
    {
        return Mesh.Scale;
    }
    public void CreateResources(Color color, Star star, Astroid astroid)
    {
        // Create 3 resource meshes in a triangle formation
        for (int i = 0; i < 3; i++)
        {
            ResourceMesh resourceMesh = new ResourceMesh
            {
                Scale = 0.8f,
                PrimaryColor = color,
                VariationAmount = 0.3f
            };
            if (star != null)
            {
                star.AddChild(resourceMesh);
            }


            // Position in a triangle formation
            float angle = (float)(2 * Math.PI * i / 3); // 120 degrees apart
            Vector2 offset = new Vector2(
                (float)Math.Cos(angle) * 10f,
                (float)Math.Sin(angle) * 10f
            );
            resourceMesh.Position = offset + astroid.Position;

            // Add some random rotation for visual variety
            if (resourceMesh.AddRandomRotation)
            {
                resourceMesh.RotationDegrees = (float)GD.RandRange(0, 360);
            }
            Console.WriteLine($"Resource mesh created at position: {resourceMesh.Position}");
        }
    }
}
