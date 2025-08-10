using Godot;
using System;
[GlobalClass]
public partial class OpenModuleSelect : Panel
{
    [Export]
    public MeshInstance2D mesh;
    [Export]
    public Color color;
    [Export]
    public Button button;
    [Export]
    public PackedScene moduleScene;
    
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        mesh.Modulate = color;
        button.ButtonUp += ButtonPressed;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void ButtonPressed()
    {
        Canvas Canvas = GetParent().GetParent<Canvas>();
        if (Canvas.module != null)
        {
            Canvas.module.QueueFree();
            Canvas.module = null;
        }
        if (Canvas.moduleScene == null || Canvas.moduleScene != moduleScene)
        {
            Canvas.module = moduleScene.Instantiate<VBoxContainer>();
            Canvas.moduleScene = moduleScene;
            Canvas.AddChild(Canvas.module);
        }

    }
}
