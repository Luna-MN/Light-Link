using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class ShipSave : Godot.Resource
{
    [Export] public Godot.Collections.Array<Vector2> NodePositions { get; set; } = new();
    [Export] public Godot.Collections.Array<int> NodeTypes { get; set; } = new();

    // Store triangle data
    [Export] public Godot.Collections.Array<int> DefineTriangles { get; set; } = new();
    [Export] public Godot.Collections.Array<Color> TriangleColors { get; set; } = new();
}