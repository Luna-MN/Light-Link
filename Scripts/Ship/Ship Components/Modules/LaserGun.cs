using Godot;
using System;
using System.Linq;

public partial class LaserGun : Gun
{
    public LaserGun(ModuleUI.GunName laserName = ModuleUI.GunName.Laser) : base(laserName)
    {
    }
    

    public override void Placed(PlayerCreatedShip ship)
    {
        base.Placed(ship);
        var bullet = new basicLaser(ship.TargetNode, this);
        GetTree().CurrentScene.AddChild(bullet);
        GD.Print("Laser Made");
    }

    public override void Fire()
    {

    }
}
