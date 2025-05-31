using System.Collections;
using System.Collections.Generic;
using Godot;

public partial class ResourcePlace : Building
{
    public override void _Ready()
    {
        base._Ready();
        AddToGroup("ResourceBuildings");
        Mesh = new MeshInstance2D();
        Mesh.Mesh = new SphereMesh();
        Mesh.Scale = new Vector2(10f, 10f);
        AddChild(Mesh);
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}
