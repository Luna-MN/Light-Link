using Godot;
using System;
using System.Linq;
using System.Reflection;
[GlobalClass]
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
		Utility
	};
	[Export]
	public ModuleName moduleName;
	
	public enum GunName
	{
		Laser,
		Rocket,
		Missile,
		Plasma,
		Ion,
		IonCannon,
		PlasmaCannon,
		PlasmaRailgun,
		IonRailgun,
	};

	[Export] public GunName gunName;
	
	public enum ShieldName
	{
		physical,
		light,
		plasma,
	};

	[Export] public ShieldName shieldName;
	
	public enum UtilityName
	{
		Engine,
		Oxygen,
		AttachmentPoint
	};
	
	[Export] public UtilityName utilityName;

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
				mod = CreateGun(gunName);
				break;
			case ModuleName.Shield:
				mod = new Shield(shieldName);
				break;
			case ModuleName.Utility:
				break;
		}

		if (((Camera)GetViewport().GetCamera2D()).ships.Count == 1)
		{
			mod.moduleName = moduleName;
			((Camera)GetViewport().GetCamera2D()).ships[0].AddChild(mod);
		}
		else
		{
			mod.QueueFree();
			mod = null;
		}
	}

	public Module CreateGun(GunName gunName)
	{
		Module mod = null;
		switch (gunName)
		{
			case GunName.Laser:
				mod = new LaserGun();
				break;
			case GunName.Rocket:
				mod = new Gun(gunName);
				break;
			case GunName.Missile:
				mod = new MissileGun();
				break;
			case GunName.Plasma:
				mod = new PlasmaGun();
				GD.Print("plasma");
				break;
			case GunName.Ion:
				mod = new IonGun();
				break;
			case GunName.IonCannon:
				mod = new IonMissile();
				break;
		}
		return mod;
	}
}
