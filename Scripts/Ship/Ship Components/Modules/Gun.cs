using Godot;
using System;

public partial class Gun : Module
{
    [Export] public PackedScene MeshScene;
    public Node2D MeshParent;
    public override void _Ready()
    {
        meshParent = MeshScene.Instantiate<Node2D>();
        base._Ready();
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}
