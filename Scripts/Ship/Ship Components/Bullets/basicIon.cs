using Godot;
using System;

public partial class basicIon : basicBullet
{
    public Vector2 Direction = Vector2.Right; // Set this to your gun's firing direction
    public Vector2 GunTipPosition;

    const float BASE_RADIUS = 0.5f; // If your original circle diameter is 1

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
            Position = GunTipPosition + Direction.Normalized() * (BASE_RADIUS * Scale.X);
        }
    }
}