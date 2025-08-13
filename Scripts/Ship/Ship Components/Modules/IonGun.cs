using Godot;
using System;

public partial class IonGun() : Gun(ModuleUI.GunName.Ion)
{
    public override void _Ready()
    {
        base._Ready();
        FireTimer.WaitTime = 5f;
        BulletMeshPath = "res://Meshs/Bullets/basicIon.tscn";
    }
}
