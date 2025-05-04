using Godot;
using System;

public partial class Body : Node2D
{
	public LowPolyMesh Mesh;
	public enum MeshType
	{
		Star,
		Planet,
		Astroid,
		Moon,
	}
	public MeshType Type;
	public Body(MeshType type)
	{
		Type = type;
		CreateMesh(Type);
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void CreateMesh(MeshType type)
	{
		Type = type;
		switch (type)
		{
			case MeshType.Star:
				Mesh = new LowPolyStarMesh();
				AddChild(Mesh);
				break;
			case MeshType.Planet:
				Mesh = new LowPolyPlanetMesh();
				AddChild(Mesh);
				break;
			case MeshType.Astroid:
				Mesh = new LowPolyAstroidMesh();
				AddChild(Mesh);
				break;
			case MeshType.Moon:
				Mesh = new LowPolyMoonMesh();
				AddChild(Mesh);
				break;
			default:
				Mesh = new LowPolyPlanetMesh();
				AddChild(Mesh);
				break;
		}
	}
}
