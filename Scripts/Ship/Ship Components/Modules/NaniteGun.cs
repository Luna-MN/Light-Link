using Godot;
using System;

public partial class NaniteGun() : Gun (ModuleUI.GunName.Nanite)
{
    public override void _Ready()
    {
        base._Ready();
        BulletMeshPath = "res://Meshs/Bullets/Nanite.tscn";
    }
}
