using Godot;
using System;

public partial class basicMissile : basicBullet
{
    public float missileRamp = 1f;
    public override void _Ready()
    {
        speed = 10f;
        base._Ready();
        BulletTimeout.WaitTime = 4f;
        BulletTimeout.Start();
        damage = 2;
    }

    public override void MoveBullet(float time)
    {
        speed += missileRamp;
        base.MoveBullet(time);
    }

    public override void OnBulletHit(Node2D Body)
    {
        // release smoke
        base.OnBulletHit(Body);
    }
}
