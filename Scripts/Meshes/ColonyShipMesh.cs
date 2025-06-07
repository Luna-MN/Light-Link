using Godot;
using System;
using System.Collections.Generic;

public partial class ColonyShipMesh : ShipMesh
{
    public override List<Vector3> DefineVertices()
    {
        return new List<Vector3>
        {
            // Front tip (sharp triangular nose)
            new Vector3(0.5f, 0f, 0),        // 0: Front tip
            new Vector3(0.0f, -0.3f, 0),     // 1: Front lower joint
            new Vector3(0.0f, 0.3f, 0),      // 2: Front upper joint

            // Rear body (engine area)
            new Vector3(-0.5f, -0.2f, 0),    // 3: Rear lower joint
            new Vector3(-0.5f, 0.2f, 0),     // 4: Rear upper joint
            new Vector3(-0.2f, 0f, 0),       // 5: Rear engine tip

            // Right wing
            new Vector3(0.0f, -0.3f, 0),    // 6: Right wing front base
            new Vector3(-0.5f, -0.5f, 0),    // 7: Right wing tip
            new Vector3(-0.5f, -0.2f, 0),    // 8: Right wing rear connection (same as vertex 3)

            // Left wing
            new Vector3(0.0f, 0.3f, 0),     // 9: Left wing base
            new Vector3(-0.5f, 0.5f, 0),     // 10: Left wing tip
            new Vector3(-0.5f, 0.2f, 0),     // 11: Left wing rear connection (same as vertex 4)
        };
    }

    protected override List<int> DefineTriangles()
    {
        return new List<int>
        {
            // Front triangular body
            0, 1, 2,

            // Middle body
            1, 3, 4,
            1, 4, 2,

            // Rear engine (distinctive triangular shape)
            3, 5, 4,

            // Right wing (connected at two points: vertices 6,7,8)
            6, 7, 8,

            // Left wing
            9, 11, 10
        };
    }

    protected override List<Color> DefineColors()
    {
        return new List<Color>
        {
            BodyColor,                   // Front triangular body
            BodyColor,                   // Middle body
            BodyColor,                   // Middle body
            CockpitColor, // Rear engine (distinctive blue color)
            WingColor,                   // Right wing
            WingColor                    // Left wing
        };
    }
}