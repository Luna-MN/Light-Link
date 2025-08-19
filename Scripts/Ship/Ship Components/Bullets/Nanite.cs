using Godot;
using System;
using System.Linq;
using System.Net.Mail;

public partial class Nanite : basicBullet
{
    [Signal]
    public delegate void NaniteHitEventHandler(Node2D Ship, Nanite Bullet, float Damage);
    public Ship Ship;
    public bool attached = false;
    public override void _Ready()
    {
        base._Ready();
        damage = 0.5f;
    }

    public override void BulletRegistration()
    {
        GetTree().Root.GetChildren().FirstOrDefault()?.GetNode<HitDetector>("HitDetector").RegisterNanite(this);
    }

    public override void _Process(double delta)
    {
        if (!attached)
        {
            base._Process(delta);
        }
        else 
        {

            if (target != null)
            {
                var targetPoint = ((AttachmentPoint)target);

                targetPoint.Health -= damage;
                if (targetPoint.Health <= 0)
                {
                    targetPoint.QueueFree();
                    targetPoint = null;
                    attached = false;
                }
            }

            Ship = ((AttachmentPoint)target).ship;
            if (Ship != null)
            {
                Ship.health -= damage;
                GD.Print(Ship.health);
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
        EmitSignal("NaniteHit", Body.GetParent<Node2D>(), this, damage);
    }
}
