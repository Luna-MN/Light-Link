using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class AstroidContextMenu : ContextMenu
{
    public Astroid astroid;
    public MainShip mainShip;
    public AstroidContextMenu(Astroid astroid)
    {
        this.astroid = astroid;
    }
    public override void _Ready()
    {
        base._Ready();
        mainShip = GetTree().Root.FindChild("MainShip", true, false) as MainShip;
        // Add menu items
        if (astroid.GlobalPosition.DistanceTo(mainShip.GlobalPosition) < mainShip.MineDistance)
        {
            AddItem("Mine Astroid", MineAstroidMyself);
        }
        else if (astroid.GlobalPosition.DistanceTo(mainShip.GlobalPosition) > mainShip.MineDistance)
        {
            AddDisabledItem("Mine Astroid", "Too Far Away");
        }

        AddItem("Move To Astroid", MoveToAstroid);

    }
    public override void _Process(double delta)
    {
        base._Process(delta);
    }
    public void MineAstroidMyself()
    {
        mainShip.startMining(astroid);
    }
    public void MoveToAstroid()
    {
        if (mainShip == null || astroid == null)
            return;

        // Calculate proper approach distance based on asteroid size
        float approachDistance = astroid.Properties.Radius + mainShip.GetApproachDistance();

        // Get direction vector from asteroid to ship
        Vector2 directionFromAsteroid = (mainShip.GlobalPosition - astroid.GlobalPosition).Normalized();

        // If ship is too far away, use a random approach angle instead
        if (mainShip.GlobalPosition.DistanceTo(astroid.GlobalPosition) > approachDistance * 3)
        {
            float randomAngle = (float)GD.RandRange(0, Mathf.Pi * 2);
            directionFromAsteroid = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
        }

        // Calculate ideal position at the appropriate distance from the asteroid
        Vector2 targetPos = astroid.GlobalPosition + (directionFromAsteroid * approachDistance);

        // Add small random offset to prevent ships stacking at exactly the same position
        targetPos += new Vector2(
            (float)GD.RandRange(-5, 5),
            (float)GD.RandRange(-5, 5)
        );

        // Set the ship's target position
        mainShip.targetPosition = targetPos;

    }

}