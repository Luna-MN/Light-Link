using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class Gun : Module
{
    public Type BulletType = typeof(basicBullet);
    public string[] paths = new string[]
    {
        "res://Meshs/Modules/SimpleLaserMesh.tscn",
        "res://Meshs/Modules/SimpleGunMesh.tscn",
        "res://Meshs/Modules/SimpleMissileMesh.tscn",
        "res://Meshs/Modules/SimpleLaserMesh.tscn",
        "res://Meshs/Modules/SimpleIonMesh.tscn",
        "res://Meshs/Modules/SimpleMissileMesh.tscn",
        "res://Meshs/Modules/SimpleGunMesh.tscn",
    };
    private string path = "res://Meshs/Modules/SimpleGunMesh.tscn";
    public Timer FireTimer;
    public string BulletMeshPath = "res://Meshs/Bullets/basicBullet.tscn";
    public Node2D target;
    public AttachmentPoint targetPoint;
    public Node2D bulletPlace;
    public Node2D Barrel;
    
    // Recoil settings
    public float RecoilDistance = 6f;
    public float RecoilBackTime = 0.05f;
    public float RecoilReturnTime = 0.12f;
    
    private Vector2 barrelHomeLocal;
    private Tween recoilTween;

    public Gun(ModuleUI.GunName gunName)
    {
        if ((int)gunName < paths.Length)
        {
            path = paths[(int)gunName];
        }
    }
    public override void _Ready()
    {
        var MeshScene = GD.Load<PackedScene>(path);
        meshParent = MeshScene.Instantiate<Node2D>();
        meshParent.Scale = new Vector2(0.5f, 0.5f);
        AddChild(meshParent);
        base._Ready();
        FireTimer = new Timer()
        {
            Autostart = true,
            OneShot = false,
            WaitTime = 0.5f,
        };
        AddChild(FireTimer);
        FireTimer.Timeout += Fire;
        bulletPlace = meshParent.GetNode<Node2D>("BulletPlace");
        Barrel = meshParent.GetNode<Node2D>("Barrel");
        
        // Cache resting position in local space (so it follows parent movement/rotation)
        if (Barrel != null)
            barrelHomeLocal = Barrel.Position;
        
        ZIndex = 12;

    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (placed)
        {
            try
            {
                AttachmentPoint closestNode = ((PlayerCreatedShip)target).shipNodes[0];
                foreach (var node in ((PlayerCreatedShip)target).shipNodes)
                {
                    if (node.GlobalPosition.DistanceTo(GlobalPosition) <
                        closestNode.GlobalPosition.DistanceTo(GlobalPosition))
                    {
                        closestNode = node;
                    }
                }

                targetPoint = closestNode;
                LookAt(targetPoint.GlobalPosition);
            }
            catch (Exception e)
            {
                target = null;
                return;
            }
        }
    }

    public override void Placed(PlayerCreatedShip ship)
    {
        // Your custom logic for Gun when placed, e.g.:
        GD.Print("Gun placed!");
        ship.guns.Add(this);
        // Call the base implementation if needed
        base.Placed(ship);
    }
    public virtual void Fire()
    {
        if (placed && target != null)
        {
            DoRecoil();

            var bulletScene = GD.Load<PackedScene>(BulletMeshPath);
            var bullet = bulletScene.Instantiate<basicBullet>();
            bullet.Scale = new Vector2(3.3f, 1);
            bullet.gun = this;
            bullet.target = targetPoint;
            bullet.Rotation = Rotation;
            bullet.GlobalPosition = bulletPlace.GlobalPosition;
            GetTree().Root.GetChildren().FirstOrDefault()?.AddChild(bullet);
        }
    }
    private void DoRecoil()
    {
        if (Barrel == null)
            return;

        if (GodotObject.IsInstanceValid(recoilTween))
        {
            recoilTween.Kill();
            recoilTween = null;
        }

        // Local "back" direction = opposite of the bullet's exit direction.
        // Assumes Barrel and BulletPlace share the same parent (meshParent).
        Vector2 backLocal;
        if (bulletPlace != null && Barrel.GetParent() == bulletPlace.GetParent())
            backLocal = (Barrel.Position - bulletPlace.Position).Normalized();
        else
            backLocal = new Vector2(-1, 0); // Fallback: -X in local space

        var backTarget = barrelHomeLocal + backLocal * RecoilDistance;

        recoilTween = CreateTween();
        recoilTween.SetParallel(false);
        recoilTween.TweenProperty(Barrel, "position", backTarget, RecoilBackTime)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
        recoilTween.TweenProperty(Barrel, "position", barrelHomeLocal, RecoilReturnTime)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.In);
    }

}
