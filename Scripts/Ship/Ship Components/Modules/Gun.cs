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
    };
    private string path = "res://Meshs/Modules/SimpleGunMesh.tscn";
    public Timer FireTimer;
    public string BulletMeshPath = "res://Meshs/Bullets/basicBullet.tscn";
    public Node2D target;
    public Node2D bulletPlace;
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
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
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
            var bulletScene = GD.Load<PackedScene>(BulletMeshPath);
            var bullet = bulletScene.Instantiate<basicBullet>();
            bullet.gun = this;
            bullet.Scale = new Vector2(1f, 1f);
            bullet.target = target;
            bullet.Rotation = Rotation;
            bullet.GlobalPosition = bulletPlace.GlobalPosition;
            GetTree().Root.GetChildren().FirstOrDefault()?.AddChild(bullet);
        }
    }
}
