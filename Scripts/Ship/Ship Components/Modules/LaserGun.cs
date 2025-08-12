using Godot;
using System;
using System.Linq;
[GlobalClass]
public partial class LaserGun : Gun
{
    public LaserGun(ModuleUI.GunName laserName = ModuleUI.GunName.Laser) : base(laserName)
    {
    }
    

    public override void Placed(PlayerCreatedShip ship)
    {
        base.Placed(ship);
        laserBeam(ship);
    }

    public virtual void laserBeam(PlayerCreatedShip ship)
    {
        var bullet = new basicLaser(ship.TargetNode, this);
        GetTree().CurrentScene.AddChild(bullet);
    }

    public override void Fire()
    {

    }
}
