using Godot;
using System;
using System.Collections.Generic;

public partial class HitDetector : Node2D
{
    private List<Node2D> Ships = new();
    private List<basicBullet> bullets = new();
    private List<Nanite> nanites = new();

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

    public void RegisterNanite(Nanite nanite)
    {
        nanites.Add(nanite);
        nanite.NaniteHit += NaniteHit;
    }
    public void DropNanite(Nanite nanite)
    {
        nanites.Remove(nanite);
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
                        if (node.GlobalPosition.DistanceTo(bullet.GlobalPosition) < closestNode.GlobalPosition.DistanceTo(bullet.GlobalPosition))
                        {
                            closestNode = node;
                        }
                    }

                    closestNode.Health -= damage;
                    if (closestNode.Health <= 0)
                    {
                        PCShip.shipNodes.Remove(closestNode);
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

    public void NaniteHit(Node2D Ship, Nanite nanite, float Damage)
    {
        if (nanites.Contains(nanite))
        {
            if (nanite.attached)
            {
                return;
            }
            nanite.GetParent().RemoveChild(nanite);
            nanite.Position = Ship.ToLocal(nanite.GlobalPosition);
            Ship.CallDeferred("AddChild", nanite);
            nanite.attached = true;
            nanites.Remove(nanite);
        }
    }
}
