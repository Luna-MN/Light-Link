using Godot;
using System;
using Godot.Collections;

public partial class basicIon : basicBullet
{
    public override void _Ready()
    {
        base._Ready();
        BulletTimeout.WaitTime = 8f;
    }
    public override void OnBulletFired()
    {
        
    }

    public override void MoveBullet(float time)
    {
        if (Scale.X >= 50f)
        {
            Scale = new Vector2(50f, 50f);
            base.MoveBullet(time);
        }
        else
        {
            Scale += new Vector2(1f, 1f);
        }
    }
}
