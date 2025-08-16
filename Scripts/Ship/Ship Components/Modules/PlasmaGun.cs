using Godot;
using System;

public partial class PlasmaGun : LaserGun
{
    public PlasmaGun() : base(ModuleUI.GunName.Plasma)
    {
        
    }

    public override void laserBeam(PlayerCreatedShip ship)
    {
        var bullet = new plasmaLaser(target , this, ship.TargetNode);
        GetTree().CurrentScene.AddChild(bullet);
    }
}
