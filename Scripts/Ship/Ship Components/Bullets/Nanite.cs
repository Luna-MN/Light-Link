using Godot;
using System;
using System.Net.Mail;

public partial class Nanite : basicBullet
{
    [Signal]
    public delegate void NaniteHitEventHandler(Node2D Ship, Nanite Bullet, float Damage);
    public Ship Ship;
    public AttachmentPoint AttachmentPoint;
    public bool attached = false;
    public override void _Ready()
    {
        base._Ready();
        damage = 0.5f;
    }

    public override void BulletRegistration()
    {
        base.BulletRegistration();
    }

    public override void _Process(double delta)
    {
        if (!attached)
        {
            base._Process(delta);
        }
        else 
        {
            if (AttachmentPoint != null)
            {
                AttachmentPoint.Health -= damage;
                if (AttachmentPoint.Health <= 0)
                {
                    AttachmentPoint.QueueFree();
                    AttachmentPoint = null;
                    attached = false;
                }
            }

            if (Ship != null)
            {
                Ship.health -= damage;
                if (Ship.health <= 0)
                {
                    Ship.QueueFree();
                    Ship = null;
                }
            }
        }
    }

    public override void OnBulletHit(Node2D Body)
    {
        EmitSignal("naniteHit", Body.GetParent<Node2D>(), this, damage);
    }
}
