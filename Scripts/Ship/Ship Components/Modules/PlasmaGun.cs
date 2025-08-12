using Godot;
using System;

public partial class PlasmaGun : LaserGun
{
    public PlasmaGun() : base(ModuleUI.GunName.Plasma)
    {
        
    }

    public override void Placed(PlayerCreatedShip ship)
    {
        additive = true;
        base.Placed(ship);
        var bullet = new plasmaLaser(ship.TargetNode, this);
        GetTree().CurrentScene.AddChild(bullet);
    }
}
