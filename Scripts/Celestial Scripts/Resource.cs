using Godot;
using System;

public partial class Resource : Node2D
{
	public Color color = Colors.White;
	public Properties.Type type;
	public Vector2 startPosition;
	public ResourceShip resourceShip;
	public bool isAttaching = false;
	public bool isAttached = false;
	public Resource(Properties.Type type)
	{
		this.type = type;
		SetAstroidType();
		// Create a ResourceMesh for the resource
		ResourceMesh resourceMesh = new ResourceMesh
		{
			Scale = 1.0f,
			PrimaryColor = color,
			VariationAmount = new RandomNumberGenerator().RandfRange(0.01f, 1f),
		};
		AddChild(resourceMesh);
		AddToGroup("Resources");
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		moveToStartPosition((float)delta);
	}
	public void SetAstroidType()
	{
		switch (type)
		{
			case Properties.Type.Rock:
				color = Colors.Brown;
				break;
			case Properties.Type.Ice:
				color = Colors.LightBlue;
				break;
			case Properties.Type.Iron:
				color = Colors.Gray;
				break;
			case Properties.Type.Carbon:
				color = Colors.DarkGray;
				break;
			default:
				color = Colors.White;
				break;
		}
	}
	public void moveToStartPosition(float delta)
	{
		if (startPosition != Vector2.Inf)
		{
			if (startPosition != Position)
			{
				Position = Position.Lerp(startPosition, delta * 2);
			}
			else
			{
				startPosition = Vector2.Inf;
			}
		}
	}
}
