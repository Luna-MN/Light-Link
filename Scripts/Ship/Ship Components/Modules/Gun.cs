using Godot;
using System;
[GlobalClass]
public partial class Gun : Module
{
    public override void _Ready()
    {
        var MeshScene = GD.Load<PackedScene>("res://Meshs/Modules/SimpleGunMesh.tscn");
        meshParent = MeshScene.Instantiate<Node2D>();
        meshParent.Scale = new Vector2(0.5f, 0.5f);
        AddChild(meshParent);
        base._Ready();
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}
