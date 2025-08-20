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
    private Timer NaniteDestroyTimer;
    public override void _Ready()
    {
        base._Ready();
        damage = 0.5f;
        NaniteDestroyTimer = new Timer()
        {
            Autostart = false,
            OneShot = true,
            WaitTime = 1f
        };
        NaniteDestroyTimer.Timeout += NaniteDestroy; 
        AddChild(NaniteDestroyTimer);
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
                if (targetPoint.Nanited == false)
                {
                    targetPoint.Nanited = true;
                    NaniteDestroyTimer.Start();
                }
            }
        }
    }
    private void NaniteDestroy()
    {
        var targetPoint = ((AttachmentPoint)target);
        targetPoint.Nanited = false;
        QueueFree();
    }

    public override void OnBulletHit(Node2D Body)
    {
        EmitSignal("NaniteHit", Body.GetParent<Node2D>(), this, damage);
    }
}
