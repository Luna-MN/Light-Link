using Godot;
using System;

public partial class basicMissile : basicBullet
{
    public override void _Ready()
    {
        speed = 10f;
        base._Ready();
        BulletTimeout.WaitTime = 4f;
    }

    public override void MoveBullet(float time)
    {
        speed += 0.001f;
        base.MoveBullet(time);
    }
}
