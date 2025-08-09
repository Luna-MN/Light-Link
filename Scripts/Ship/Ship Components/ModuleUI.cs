using Godot;
using System;
using System.Reflection;

public partial class ModuleUI : Panel
{
	[Export]
	public MeshInstance2D mesh;
	[Export]
	public Color color;
	[Export]
	public Button button;
	[Export]
	public PackedScene createScene;
	
	public Action OnButtonPressed;

	public enum ModuleName
	{
		Gun,
		Shield,
		Engine
	};
	[Export]
	public ModuleName moduleName;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		mesh.Modulate = color;
		button.Pressed += ButtonPressed;
		button.Pressed += OnButtonPressed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void ButtonPressed()
	{
		Module mod = null;
		switch (moduleName)
		{
			case ModuleName.Gun:
				mod = new Gun();
				break;
			case ModuleName.Shield:
				break;
			case ModuleName.Engine:
				break;
		}
		
		GetTree().Root.AddChild(mod);
	}
}
