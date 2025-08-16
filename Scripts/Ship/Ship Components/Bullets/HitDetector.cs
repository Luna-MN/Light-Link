using Godot;
using System;
using System.Collections.Generic;

public partial class HitDetector : Node2D
{
    private List<Node2D> Ships = new();
    private List<basicBullet> bullets = new();

    public void RegisterShip(Node2D Body)
    {
        Ships.Add(Body);
    }

    public void DropShip(Node2D Body)
    {
        if (Ships.Contains(Body))
        {
            Ships.Remove(Body);
        }
    }
    public void RegisterBullet(basicBullet Body)
    {
        bullets.Add(Body);
        Body.BulletHit += BulletHit;
    }
    public void DropBullet(basicBullet Body)
    {
        if (bullets.Contains(Body))
        {
            bullets.Remove(Body);
        }
    }

    public void BulletHit(Node2D Body, basicBullet bullet, float damage)
    {
        if (Ships.Contains(Body) && bullets.Contains(bullet))
        {
            if (Body is Ship ship)
            {
                ship.health -= damage;
                GD.Print(ship.health);
                // deal damage to the closest ship Node
                if (ship is PlayerCreatedShip PCShip)
                {
                    var closestNode = PCShip.shipNodes[0];
                    foreach (var node in PCShip.shipNodes)
                    {
                        if (node.GlobalPosition.DistanceTo(GlobalPosition) <
                            closestNode.GlobalPosition.DistanceTo(GlobalPosition))
                        {
                            closestNode = node;
                        }
                    }

                    closestNode.Health -= damage;
                    if (closestNode.Health <= 0)
                    {
                        closestNode.QueueFree();
                    }
                }
                // if the ship is dead, destroy it
                if (ship.health <= 0)
                {
                    ship.QueueFree();
                }
            }
        }

        if (bullet is not basicLaser)
        {
            DropBullet(bullet);
        }

    }
}
