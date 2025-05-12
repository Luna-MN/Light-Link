using Godot;
using System;
using System.Collections.Generic;

public partial class GasSkimmerMesh : ShipMesh
{
    [Export] public float ProbeLengthFactor = 1.0f; // Controls how long the bottom probe is
    [Export] public float BevelSize = 0.3f; // Controls the size of the bevel transition

    protected override List<Vector3> DefineVertices()
    {
        float probeLength = 2.0f * ProbeLengthFactor;

        return new List<Vector3>
        {
            // Top triangular section
            new Vector3(-1.5f, -0.5f, 0),  // 0: Top left point
            new Vector3(1.5f, -0.5f, 0),   // 1: Top right point
            new Vector3(0, 0.5f, 0),       // 2: Bottom middle point of the triangular part
            
            // Main body section
            new Vector3(-1.7f, 0.5f, 0),   // 3: Left edge of main body (expanded)
            new Vector3(1.7f, 0.5f, 0),    // 4: Right edge of main body (expanded)
            new Vector3(-1.7f, 1.0f, 0),   // 5: Bottom left of main body
            new Vector3(1.7f, 1.0f, 0),    // 6: Bottom right of main body
            
            // Bevel connection points (between main body and probe)
            new Vector3(-1.0f, 1.0f + BevelSize, 0),  // 7: Left bevel point
            new Vector3(1.0f, 1.0f + BevelSize, 0),   // 8: Right bevel point
            
            // Middle section/connector to probe
            new Vector3(-0.5f, 1.0f + BevelSize, 0),   // 9: Left edge of connector
            new Vector3(0.5f, 1.0f + BevelSize, 0),    // 10: Right edge of connector
            
            // Probe section (extendable)
            new Vector3(-0.5f, 1.0f + BevelSize + probeLength, 0),      // 11: Bottom left of probe
            new Vector3(0.5f, 1.0f + BevelSize + probeLength, 0),       // 12: Bottom right of probe
            
            // Engine nozzles
            new Vector3(-0.4f, 1.0f + BevelSize + probeLength, 0),      // 13: Left engine top 
            new Vector3(-0.1f, 1.0f + BevelSize + probeLength, 0),      // 14: Left engine top inner
            new Vector3(-0.4f, 1.0f + BevelSize + probeLength + 0.3f, 0), // 15: Left engine bottom
            new Vector3(-0.1f, 1.0f + BevelSize + probeLength + 0.3f, 0), // 16: Left engine bottom inner
            
            new Vector3(0.1f, 1.0f + BevelSize + probeLength, 0),       // 17: Right engine top inner
            new Vector3(0.4f, 1.0f + BevelSize + probeLength, 0),       // 18: Right engine top
            new Vector3(0.1f, 1.0f + BevelSize + probeLength + 0.3f, 0),  // 19: Right engine bottom inner
            new Vector3(0.4f, 1.0f + BevelSize + probeLength + 0.3f, 0),  // 20: Right engine bottom
            
            // Side details (antenna/sensors)
            new Vector3(-1.7f, 0.7f, 0),   // 21: Left side detail
            new Vector3(-2.0f, 0.7f, 0),   // 22: Left side detail tip
            new Vector3(1.7f, 0.7f, 0),    // 23: Right side detail
            new Vector3(2.0f, 0.7f, 0),    // 24: Right side detail tip
        };
    }

    protected override List<int> DefineTriangles()
    {
        return new List<int>
        {
            // Top triangular section (blue)
            0, 2, 1,
            
            // Side connections between cockpit and body
            0, 3, 2,  // Left side connection
            1, 2, 4,  // Right side connection
            
            // Main body - top connecting section
            2, 3, 5,
            2, 5, 7,
            2, 7, 10,
            2, 10, 8,
            2, 8, 6,
            2, 6, 4,
            
            // Main body - rectangle
            3, 4, 5,
            4, 6, 5,
            
            // Beveled transition - left side
            5, 7, 9,
            
            // Beveled transition - right side
            8, 10, 6,
            
            // Probe section
            9, 11, 12,
            9, 12, 10,
        
            
            // Side details - antenna/sensors
            21, 22, 5, // Left antenna
            6, 24, 23, // Right antenna
        };
    }

    protected override List<Color> DefineColors()
    {
        return new List<Color>
        {
            CockpitColor,    // Top triangular section (blue)
            
            BodyColor,       // Left side connection
            BodyColor,       // Right side connection

            BodyColor,       // Main body connectors
            BodyColor,
            BodyColor,
            BodyColor,
            BodyColor,
            BodyColor,

            BodyColor,       // Main body rectangle
            BodyColor,

            BodyColor,       // Beveled transition - left
            BodyColor,       // Beveled transition - right

            BodyColor,       // Probe section
            BodyColor,

            BodyColor,       // Left antenna
            BodyColor,       // Right antenna
        };
    }
}