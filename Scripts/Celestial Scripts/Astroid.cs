using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
public partial class Astroid : Body
{
	public AstroidProperties Properties;
	public Control selectionBrackets;

	public Astroid(AstroidProperties properties, MeshType type = MeshType.Astroid) : base(type)
	{
		Properties = properties;
		Mesh.Scale = new Vector2(properties.Radius, properties.Radius);
		SetAstroidColor();
		AddToGroup("Astroids");
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CreateSelectionBrackets();
		ShowSelectionBrackets(false);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void SetAstroidColor()
	{
		Color color;

		switch (Properties.AstroidType)
		{
			case global::Properties.Type.Rock:
				color = Colors.Brown;
				break;
			case global::Properties.Type.Ice:
				color = Colors.LightBlue;
				break;
			case global::Properties.Type.Iron:
				color = Colors.Gray;
				break;
			case global::Properties.Type.Carbon:
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
		((LowPolyAstroidMesh)Mesh).AddRandomColorVariation(colors, false);  // false = don't preserve water/land distinction

		// Apply the colors
		Mesh.SetTriangleColors(colors);
	}
	private void CreateSelectionBrackets()
	{
		selectionBrackets = new Control();
		selectionBrackets.Name = "SelectionBrackets";
		AddChild(selectionBrackets);

		// Create smaller brackets
		CreateBracket("TopLeftBracket", new Vector2(-1, -1), new Vector2(-0.5f, -1), new Vector2(-1, -0.5f));
		CreateBracket("TopRightBracket", new Vector2(1, -1), new Vector2(0.5f, -1), new Vector2(1, -0.5f));
		CreateBracket("BottomLeftBracket", new Vector2(-1, 1), new Vector2(-0.5f, 1), new Vector2(-1, 0.5f));
		CreateBracket("BottomRightBracket", new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(1, 0.5f));
	}

	private void CreateBracket(string name, Vector2 corner, Vector2 horizontalEnd, Vector2 verticalEnd)
	{
		var bracket = new Line2D();
		bracket.Name = name;
		bracket.Width = 0.5f;
		bracket.DefaultColor = Colors.White;
		// Create smaller brackets (reduce from 40 to 10)
		bracket.AddPoint(corner * 5);
		bracket.AddPoint(horizontalEnd * 5);
		bracket.AddPoint(corner * 5);
		bracket.AddPoint(verticalEnd * 5);

		selectionBrackets.AddChild(bracket);
	}
	public void ShowSelectionBrackets(bool isVisible)
	{
		if (selectionBrackets == null)
		{
			CreateSelectionBrackets();
		}
		selectionBrackets.Visible = isVisible;
	}
}
