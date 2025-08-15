using Godot;
using System;

public partial class IonMissile : MissileGun
{
    public override void _Ready()
    {
        base._Ready();
        BulletMeshPath = "res://Meshs/Bullets/Ionbullet.tscn";
    }
}
