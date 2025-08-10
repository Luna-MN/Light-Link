using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class Gun : Module
{
    [Export] public string[] paths;
    private string path = "res://Meshs/Modules/SimpleGunMesh.tscn";
    public Gun(ModuleUI.GunName gunName)
    {
        if ((int)gunName < paths.Length)
        {
            path = paths[(int)gunName];
        }
    }
    public override void _Ready()
    {
        var MeshScene = GD.Load<PackedScene>(path);
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
