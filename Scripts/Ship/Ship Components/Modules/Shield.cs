using Godot;
using System;
[GlobalClass]
public partial class Shield : Module
{
    [Export] public string[] paths = new string[]
    {
        "res://Meshs/Modules/SimpleShieldMesh.tscn",
    };
    private string path = "res://Meshs/Modules/SimpleShieldMesh.tscn";
    
    public Shield(ModuleUI.ShieldName shieldName)
    {
        if ((int)shieldName < paths.Length)
        {
            path = paths[(int)shieldName];
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
