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
        if (Scale.X >= 15f)
        {
            Scale = new Vector2(15f, 15f);
            base.MoveBullet(time);
        }
        else
        {
            Scale += new Vector2(1f, 1f);

            // Get muzzle tip in world space
            GunTipPosition = gun.bulletPlace.GlobalPosition;

            // Use the muzzle's +X axis as the firing direction (world space).
            // If your muzzle is modeled to face "up", change to: Direction = -gun.bulletPlace.GlobalTransform.Y;
            Direction = gun.bulletPlace.GlobalTransform.X;

            // Place using world space because GunTipPosition is global
            GlobalPosition = GunTipPosition + Direction.Normalized() * (0.5f * Scale.X);
        }
    }
}