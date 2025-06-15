using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class ShipSave : Godot.Resource
{
    [Export] public Godot.Collections.Array<Vector2> NodePositions { get; set; } = new();
    [Export] public Godot.Collections.Array<int> NodeTypes { get; set; } = new();
    [Export] public Godot.Collections.Array<Color> NodeColors { get; set; } = new();

    // Store line connections as pairs of indices
    [Export] public Godot.Collections.Array<int> LineStartIndices { get; set; } = new();
    [Export] public Godot.Collections.Array<int> LineEndIndices { get; set; } = new();
    [Export] public Godot.Collections.Array<Color> LineColors { get; set; } = new();

    // Store triangle data
    [Export] public Godot.Collections.Array<int> TrianglePoint1Indices { get; set; } = new();
    [Export] public Godot.Collections.Array<int> TrianglePoint2Indices { get; set; } = new();
    [Export] public Godot.Collections.Array<int> TrianglePoint3Indices { get; set; } = new();
    [Export] public Godot.Collections.Array<Color> TriangleColors { get; set; } = new();
}