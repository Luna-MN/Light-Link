using Godot;
using System;
using System.Linq;

[GlobalClass]
public partial class basicBullet : MeshInstance2D
{
    public float speed = 50f;
    public Node2D target;
    public Timer BulletTimeout;
    public bool move = false;
    public Gun gun;
    public float damage = 1;
    public Vector2 direction;
    [Signal]
    public delegate void BulletHitEventHandler(Node2D body, basicBullet bullet, float damage);
    
    [Export] public Area2D hitArea;
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
        BulletRegistration();
        hitArea.AreaEntered += OnBulletHit;
        direction = (target.GlobalPosition - Position).Normalized();
    }

    public virtual void BulletRegistration()
    {
        GetTree().Root.GetChildren().FirstOrDefault()?.GetNode<HitDetector>("HitDetector").RegisterBullet(this);
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
        
        // Move in that direction by (speed * time)
        Position += direction * speed * time;

    }
    
    public virtual void OnBulletHit(Node2D Body)
    {
        if (Body.GetParent() != gun.ship)
        {
            EmitSignal("BulletHit", Body.GetParent<Node2D>(), this, damage);
            QueueFree();
        }
    }

    public virtual void OnBulletTimeout()
    {
        QueueFree();
    }
}
