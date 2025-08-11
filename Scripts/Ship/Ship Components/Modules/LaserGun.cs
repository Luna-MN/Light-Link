using Godot;
using System;
using System.Linq;

public partial class LaserGun : Gun
{
    public LaserGun() : base(ModuleUI.GunName.Laser)
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
