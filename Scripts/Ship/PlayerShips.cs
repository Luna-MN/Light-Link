using Godot;
using System;

public partial class PlayerShips : Ship
{
    public bool DisableMovement = false;
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
