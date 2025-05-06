using Godot;
using System;

public partial class PlayerShips : Ship
{
    public override void _Ready()
    {
        AddToGroup("MyShips");
        isMine = true;
        base._Ready();
    }
    public void SetShipTarget(Vector2 pos)
    {
        if (shipSelected && isMine)
        {
            // Get the mouse position in global coordinates
            targetPosition = pos;
        }
    }
}
