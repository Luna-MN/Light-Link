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
        laser.Position = Position;
        AddChild(laser);
        base._Ready();
    }
    public override void OnBulletFired(double delta)
    {
        laser.ClearPoints();
        laser.AddPoint(Position);
        laser.AddPoint(target.GlobalPosition);
    }
    public override void OnBulletTimeout()
    {
        base.OnBulletTimeout();
    }
}
