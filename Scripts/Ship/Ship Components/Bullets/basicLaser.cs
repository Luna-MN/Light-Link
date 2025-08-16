using Godot;
using System;
[GlobalClass]
public partial class basicLaser : basicBullet
{
    public Line2D laser;
    public LaserGun gun;
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
        laser.ClearPoints();
        laser.AddPoint(gun.bulletPlace.GlobalPosition);
        laser.AddPoint(target.GlobalPosition);
    }
    public override void OnBulletTimeout()
    {
        EmitSignal("BulletHit", target, this, damage);
    }
}
