using Godot;
using System;
[GlobalClass]
public partial class basicBullet : MeshInstance2D
{
    public float speed = 50f;
    public Node2D target;
    public Timer BulletTimeout;
    public bool move = false;
    public Gun gun;
    public basicBullet(Node2D target)
    {
        this.target = target;
    }

    protected basicBullet()
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
        OnBulletFired();
    }

    public override void _Process(double delta)
    {
        MoveBullet((float)delta);
    }

    public virtual void OnBulletFired()
    {
        move = true;
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
