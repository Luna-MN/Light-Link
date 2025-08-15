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

    public void BulletHit(Node2D Body, basicBullet Bullet, int damage)
    {
        if (Ships.Contains(Body) && bullets.Contains(Bullet))
        {
            if (Body is Ship ship)
            {
                ship.health -= damage;
                GD.Print(ship.health);
                if (ship.health <= 0)
                {
                    ship.QueueFree();
                }
            }
        }
        DropBullet(Bullet);
    }
}
