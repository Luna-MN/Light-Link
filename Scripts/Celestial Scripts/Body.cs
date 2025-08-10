using Godot;
using System;
[GlobalClass]
public partial class Body : Node2D
{
	public LowPolyMesh Mesh;
	public Area2D RigidBody;
	public CollisionShape2D CollisionShape;
	public CircleShape2D CircleShape;
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

		RigidBody = new Area2D();
		Mesh.AddChild(RigidBody);

		// Position RigidBody at same position as Mesh
		RigidBody.Position = new Vector2(0, 0);

		CircleShape = new CircleShape2D();
		CollisionShape = new CollisionShape2D();
		CollisionShape.Shape = CircleShape;
		CircleShape.Radius = Mesh.Radius;
		CollisionShape.Position = new Vector2(0, 0);
		RigidBody.AddChild(CollisionShape);
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Override _Process to maintain the RigidBody position at the Mesh position
	public override void _Process(double delta)
	{
		RigidBody.Position = new Vector2(0, 0);
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
	public void UpdateRBScale(float scale)
	{

	}
}
