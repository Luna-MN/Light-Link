using Godot;
using System;
[GlobalClass]
public partial class PlayerShips : Ship
{
    public bool DisableMovement = false;
    public override void _Ready()
    {
        AddToGroup("MyShips");
        base._Ready();
    }
    public void SetShipTarget(Vector2 pos, bool clear = true)
    {
        if (shipSelected && isMine)
        {
            // Get the mouse position in global coordinates
            if (clear)
            {
                path.Clear();
            }
            path.Add(pos);
        }
    }
}
