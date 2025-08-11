using Godot;
using System;

public partial class basicLaser : basicBullet
{
    public Line2D laser;
    public basicLaser(Node2D target) : base(target)
    {
        
    }
    public basicLaser()
    {
        
    }

    public override void _Ready()
    {
        laser = new Line2D();
        laser.Width = 0.5f;
        AddChild(laser);
        base._Ready();
    }
    public override void OnBulletFired(double delta)
    {
        laser.ClearPoints();
        laser.AddPoint(GlobalPosition);
        laser.AddPoint(target.GlobalPosition);
    }
    public override void OnBulletTimeout()
    {
        base.OnBulletTimeout();
    }
}
