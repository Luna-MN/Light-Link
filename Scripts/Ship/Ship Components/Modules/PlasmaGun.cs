using Godot;
using System;

public partial class PlasmaGun : LaserGun
{
    public PlasmaGun() : base(ModuleUI.GunName.Plasma)
    {
        
    }

    public override void _Ready()
    {
        
        
    }

    public override void Placed(PlayerCreatedShip ship)
    {
        base.Placed(ship);
        
    }
}
