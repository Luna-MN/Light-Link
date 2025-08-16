using Godot;
using System;
using System.Linq;
[GlobalClass]
public partial class LaserGun : Gun
{
    private basicLaser bullet;
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
        bullet = new basicLaser(ship.TargetNode, this, ship.TargetNode);
        
        GetTree().CurrentScene.AddChild(bullet);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (placed)
        {
            bullet.target = targetPoint;
        }

    }

    public override void Fire()
    {

    }
}
