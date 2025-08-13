using Godot;
using System;

public partial class basicIon : basicBullet
{
    public Vector2 Direction = Vector2.Right; // Set this to your gun's firing direction
    public Vector2 GunTipPosition;

    public override void _Ready()
    {
        base._Ready();
        //light blue
        Modulate = new Color(0, 1, 1);
        BulletTimeout.WaitTime = 10f;
        BulletTimeout.Start();
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
            GunTipPosition = gun.bulletPlace.GlobalPosition;
            Direction = new Vector2(Mathf.Cos(gun.Rotation), Mathf.Sin(gun.Rotation));
            // Place bullet on edge, not center, as it scales
            Position = GunTipPosition + Direction.Normalized() * (0.5f * Scale.X);
        }
        GD.Print(BulletTimeout.TimeLeft);
    }
}