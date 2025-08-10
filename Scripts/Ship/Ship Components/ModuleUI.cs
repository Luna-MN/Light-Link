using Godot;
using System;
using System.Linq;
using System.Reflection;

public partial class ModuleUI : Panel
{
	[Export]
	public MeshInstance2D mesh;
	[Export]
	public Color color;
	[Export]
	public Button button;
	
	public enum ModuleName
	{
		Weapon,
		Shield,
		Engine
	};
	[Export]
	public ModuleName moduleName;
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
	public void ButtonPressed()
	{
		Module mod = null;
		switch (moduleName)
		{
			case ModuleName.Weapon:
				mod = new Gun();
				break;
			case ModuleName.Shield:
				mod = new Shield();
				break;
			case ModuleName.Engine:
				break;
		}

		if (((Camera)GetViewport().GetCamera2D()).ships.Count == 1)
		{
			mod.moduleName = moduleName;
			((Camera)GetViewport().GetCamera2D()).ships[0].AddChild(mod);
		}
		else
		{
			mod?.QueueFree();
		}
	}
}
