using Godot;
using System.Collections.Generic;

public partial class PlayerShipMesh : ShipMesh
{

    public override List<Vector3> DefineVertices()
    {
        return new List<Vector3>
        {
            new Vector3(0, -1, 0),       // 0: Front tip
            new Vector3(-0.5f, 0.3f, 0),  // 1: Left body joint
            new Vector3(0.5f, 0.3f, 0),   // 2: Right body joint
            new Vector3(0, 0.2f, 0),     // 3: Cockpit center
            new Vector3(-1.0f, 0.5f, 0),  // 4: Left wing tip
            new Vector3(1.0f, 0.5f, 0),   // 5: Right wing tip
            new Vector3(-0.3f, 0.7f, 0),  // 6: Left rear
            new Vector3(0.3f, 0.7f, 0)    // 7: Right rear
        };
    }

    protected override List<int> DefineTriangles()
    {
        return new List<int>
        {
            0, 1, 3,  // Left front
            0, 3, 2,  // Right front
            1, 6, 3,  // Left middle
            2, 3, 7,  // Right middle
            1, 4, 6,  // Left wing
            2, 7, 5,  // Right wing
            3, 6, 7   // Cockpit/rear
        };
    }

    protected override List<Color> DefineColors()
    {
        return new List<Color>
        {
            BodyColor,    // Left front
            BodyColor,    // Right front
            BodyColor,    // Left middle
            BodyColor,    // Right middle
            WingColor,    // Left wing
            WingColor,    // Right wing
            CockpitColor  // Cockpit/rear
        };
    }
}