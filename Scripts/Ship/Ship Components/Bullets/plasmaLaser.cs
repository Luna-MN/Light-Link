using Godot;
using System;

public partial class plasmaLaser : basicLaser
{
    public override void _Ready()
    {
        base._Ready();
        //light blue
        Modulate = new Color(0.2f, 0.4f, 0.8f);
    }
}
