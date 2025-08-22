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
    private float MaxSize;
    [Export]
    public MeshInstance2D NaniteGreenMesh;
    private float elapsed;
    private bool isScaling;
    public override void _Ready()
    {
        base._Ready();
        BulletTimeout.Stop();
        damage = 0.5f;
        NaniteDestroyTimer = new Timer()
        {
            Autostart = false,
            OneShot = true,
            WaitTime = 3f
        };
        NaniteDestroyTimer.Timeout += NaniteDestroy;
        AddChild(NaniteDestroyTimer);
        MaxSize = NaniteGreenMesh.Scale.X;
    }

    public override void BulletRegistration()
    {
        GetTree().Root.GetChildren().FirstOrDefault()?.GetNode<HitDetector>("HitDetector").RegisterNanite(this);
    }

    public override void _Process(double delta)
    {
        LookAt(target.GlobalPosition);
        if (isScaling)
        {
            float waitTime = (float)NaniteDestroyTimer.TimeLeft;
            float scale = (waitTime / 3f) * MaxSize;
            NaniteGreenMesh.Scale = new Vector2(scale, NaniteGreenMesh.Scale.Y);
        }
        if (!attached)
        {
            base._Process(delta);
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
        if (Body.GetParent() != gun.ship && Body.GetParent() is not basicBullet)
        {
            EmitSignal("NaniteHit", Body.GetParent<Node2D>(), this, damage);
            if (target != null)
            {
                var targetPoint = ((AttachmentPoint)target);
                if (targetPoint.Nanited == false)
                {
                    targetPoint.Nanited = true;
                    isScaling = true;
                    NaniteDestroyTimer.Start();
                }
                else
                {
                    QueueFree();
                }
            }
        }
    }
}
