using Godot;
using System;

public partial class MissileGun : Gun
{
    public MissileGun(ModuleUI.GunName missileName = ModuleUI.GunName.Missile) : base(missileName)
    {
        
    }

    public override void _Ready()
    {
        BulletMeshPath = "res://Meshs/Bullets/basicMissile.tscn";
        base._Ready();
        FireTimer.WaitTime = 1f;
    }
}
