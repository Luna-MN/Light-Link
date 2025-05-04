using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
public partial class Astroid : Body
{
	public AstroidProperties Properties;
	public Astroid(AstroidProperties properties)
	{
		Properties = properties;
		Mesh.Scale = new Vector2(properties.Radius, properties.Radius);
		SetAstroidColor();
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void SetAstroidColor()
	{
		Color color;
		GD.Print("Astroid Color: " + Properties.Type);
		switch (Properties.Type)
		{
			case AstroidProperties.AstroidType.Rock:
				color = Colors.Brown;
				break;
			case AstroidProperties.AstroidType.Ice:
				color = Colors.LightBlue;
				break;
			case AstroidProperties.AstroidType.Iron:
				color = Colors.Gray;
				break;
			case AstroidProperties.AstroidType.Carbon:
				color = Colors.DarkGray;
				break;
			default:
				color = Colors.White;
				break;
		}

		// Get the number of triangles in the mesh
		int triangleCount = Mesh.GetTriangleCount();

		// Create a list with the same color for each triangle
		List<Color> colors = Enumerable.Repeat(color, triangleCount).ToList();

		// Add color variations to make the asteroid look more natural
		Mesh.AddRandomColorVariation(colors, false);  // false = don't preserve water/land distinction

		// Apply the colors
		Mesh.SetTriangleColors(colors);
	}
}
