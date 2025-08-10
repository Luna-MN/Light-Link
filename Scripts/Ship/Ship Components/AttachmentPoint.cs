using Godot;
using System;
[GlobalClass]
public partial class AttachmentPoint : Node2D
{
	public Ship ship;
	private MeshInstance2D mesh;
	public enum ShipNodeTypes
	{
		Weapon,
		Shield,
		Utility,
		Power,
	}
	public ShipNodeTypes NodeType;
	// Called when the node enters the scene tree for the first time.
	public AttachmentPoint()
	{
	}
	public override void _Ready()
	{
		RemoveFromGroup("Components");
		AddToGroup("AttachmentPoints");
		mesh = new MeshInstance2D();
		mesh.Mesh = new SphereMesh();
		mesh.Scale = new Vector2(5f, 5f);
		if (NodeType == ShipNodeTypes.Weapon)
		{
			mesh.Modulate = new Color(1, 0.5f, 0.5f); // Light Red for weapon nodes
		}
		else if (NodeType == ShipNodeTypes.Utility)
		{
			mesh.Modulate = new Color(0.5f, 1, 0.5f); // Light Green for utility nodes
		}
		else if (NodeType == ShipNodeTypes.Shield)
		{
			mesh.Modulate = new Color(0.5f, 0.5f, 1); // Light Blue for shield nodes
		}
		else if (NodeType == ShipNodeTypes.Power)
		{
			mesh.Modulate = new Color(1, 1, 0); // Light Yellow for power nodes
		}
		else
		{
			mesh.Modulate = Colors.White; // Default color for other types
		}
		AddChild(mesh);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
