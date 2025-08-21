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
            elapsed += (float)delta;
            float duration = (float)NaniteDestroyTimer.WaitTime;
            float t = Mathf.Clamp(elapsed / duration, 0f, 1f);
            float scale = Mathf.Lerp(MaxSize, 0f, t);
            NaniteGreenMesh.Scale = new Vector2(scale, scale);

            if (t >= 1f)
                isScaling = false;
        }

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
                    isScaling = true;
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
        if (Body.GetParent() != gun.ship && Body.GetParent() is not basicBullet)
        {
            EmitSignal("NaniteHit", Body.GetParent<Node2D>(), this, damage);
        }
    }

    private void timer_Process(double delta)
    {
        
    }
}
