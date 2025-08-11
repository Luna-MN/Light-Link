using Godot;
using System;

public partial class basicBullet : MeshInstance2D
{
    public float speed = 50f;
    public Node2D target;

    public basicBullet(Node2D target)
    {
        this.target = target;
    }

    public basicBullet()
    {
        
    }
    public override void _Process(double delta)
    {
        OnBulletFired(delta);
        base._Process(delta);
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
}
