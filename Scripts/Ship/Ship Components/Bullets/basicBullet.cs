using Godot;
using System;
[GlobalClass]
public partial class basicBullet : MeshInstance2D
{
    public float speed = 50f;
    public Node2D target;
    public Timer BulletTimeout;
    public basicBullet(Node2D target)
    {
        this.target = target;
    }

    public basicBullet()
    {
        
    }

    public override void _Ready()
    {
        BulletTimeout = new Timer()
        {
            Autostart = true,
            OneShot = false,
            WaitTime = 2f
        };
        BulletTimeout.Timeout += OnBulletTimeout;
        AddChild(BulletTimeout);
    }

    public override void _Process(double delta)
    {
        OnBulletFired(delta);
    }

    public virtual void OnBulletFired(double delta)
    {
        MoveBullet((float)delta);
    }

    public virtual void MoveBullet(float time)
    {
        // Calculate direction towards the target
        Vector2 direction = (target.GlobalPosition - Position).Normalized();
  
        // Move in that direction by (speed * time)
        Position += direction * speed * time;
    }
    
    public virtual void OnBulletHit()
    {
        
    }

    public virtual void OnBulletTimeout()
    {
        QueueFree();
    }
}
