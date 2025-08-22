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
	[Export]
	public PackedScene BulletLook;
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
		Nanite,
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
		if (BulletLook != null)
		{
			GetNode("Control/ColorRect").QueueFree();
			var newNode = BulletLook.Instantiate<Node2D>();
			GetNode("Control").AddChild(newNode);
			newNode.Position = new Vector2(20, 20);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void ButtonPressed()
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

	private Module CreateGun(GunName gunName)
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
			case GunName.Nanite:
				mod = new NaniteGun();
				break;
		}
		return mod;
	}
}
