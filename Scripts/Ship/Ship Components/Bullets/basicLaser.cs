using Godot;
using System;
using System.Linq;

[GlobalClass]
public partial class basicLaser : basicBullet
{
    public Line2D laser;
    public LaserGun gun;
    public int Distance = 300;
    private bool inRange = false;
    private Node2D targetShip;
    public basicLaser(Node2D target, LaserGun gun, Node2D targetShip) : base(target)
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
        
        BulletTimeout = new Timer()
        {
            Autostart = true,
            OneShot = false,
            WaitTime = 2f
        };
        BulletTimeout.Timeout += OnBulletTimeout;
        AddChild(BulletTimeout);
        OnBulletFired();
        GetTree().Root.GetChildren().FirstOrDefault()?.GetNode<HitDetector>("HitDetector").RegisterBullet(this);
        
        damage = 0.1f;
        BulletTimeout.WaitTime = 0.3f;
        BulletTimeout.Start();
    }
    public override void MoveBullet(float time)
    {
        if (gun.bulletPlace.GlobalPosition.DistanceTo(target.GlobalPosition) <= Distance)
        { 
            laser.ClearPoints();
            laser.AddPoint(ToLocal(gun.bulletPlace.GlobalPosition));
            laser.AddPoint(ToLocal(target.GlobalPosition));
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
            EmitSignal("BulletHit", targetShip, this, damage);
        }
    }
}
