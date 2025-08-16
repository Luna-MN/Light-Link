using Godot;
using System;
[GlobalClass]
public partial class basicLaser : basicBullet
{
    public Line2D laser;
    public LaserGun gun;
    public int Distance = 300;
    private bool inRange = false;
    public basicLaser(Node2D target, LaserGun gun) : base(target)
    {
        this.target = target;
        this.gun = gun;
    }
    public basicLaser()
    {
        
    }

    public override void _Ready()
    {
        laser = new Line2D();
        laser.Width = 1f;
        AddChild(laser);
        base._Ready();
        damage = 0.1f;
        BulletTimeout.WaitTime = 0.3f;
    }
    public override void MoveBullet(float time)
    {
        if (gun.bulletPlace.GlobalPosition.DistanceTo(target.GlobalPosition) <= Distance)
        { 
            laser.ClearPoints();
            laser.AddPoint(gun.bulletPlace.GlobalPosition);
            laser.AddPoint(target.GlobalPosition);
            inRange = true;
        }
        else
        {
            laser.ClearPoints();
            inRange = false;
        }
    }
    public override void OnBulletTimeout()
    {
        if (inRange)
        {
            EmitSignal("BulletHit", target, this, damage);
        }
    }
}
