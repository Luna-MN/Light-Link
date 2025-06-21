using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class SelectFile : Control
{
	[Export] public GridContainer FileList;
	[Export] public Button Browse, Select;
	[Export] public PackedScene FileItem;
	[Export] public TextEdit SavePath;
	private Color defaultButtonColor;
	public ShipPreview SelectedPreview;
	public string SelectedFilePath;
	public bool Save;
	private int myShipCount = 0;
	public string MyShipPath;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (!Directory.Exists("MyShips"))
		{
			Directory.CreateDirectory("res://MyShips");
		}
		FileList.CustomMinimumSize = new Vector2(600, 400); 
		FileList.AddThemeConstantOverride("h_separation", 10);
		FileList.AddThemeConstantOverride("v_separation", 10);
		
		FindFiles("res://MyShips");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void FindFiles(string path)
	{
		GD.Print($"Looking for files in: {path}");
		GD.Print($"Absolute path: {ProjectSettings.GlobalizePath(path)}");

		DirAccess dir = DirAccess.Open(path);
		if (dir == null)
		{
			GD.PrintErr($"Failed to access path: {path}");
			return;
		}

		dir.ListDirBegin();
		string fileName = dir.GetNext();

		while (!string.IsNullOrEmpty(fileName))
		{
			if (fileName.Contains("MyShip"))
			{
				myShipCount++;
			}
			// Skip directories and hidden files
			if (!dir.CurrentIsDir() && !fileName.StartsWith("."))
			{
				// Create a file item
				GD.Print("Adding file: " + fileName);
				ShipPreview fileItemInstance = (ShipPreview)FileItem.Instantiate();
				FileList.AddChild(fileItemInstance);
				fileItemInstance.Text.Text = fileName;
				defaultButtonColor = fileItemInstance.Select.Modulate;
				fileItemInstance.FilePath = path + "/" + fileName;
				fileItemInstance.Name = fileName;
				fileItemInstance.Select.ButtonDown += () =>
				{
					SelectedFilePath = fileItemInstance.FilePath;
					SelectedPreview ??= fileItemInstance;
					SelectedPreview.Select.Modulate = defaultButtonColor;
					SelectedPreview = fileItemInstance;
					SelectedPreview.Select.Modulate = Colors.Cyan;
				};
				fileItemInstance.SizeFlagsHorizontal = SizeFlags.ExpandFill;
				fileItemInstance.SizeFlagsVertical = SizeFlags.ExpandFill;
				fileItemInstance.CustomMinimumSize = new Vector2(150, 100);
			}

			fileName = dir.GetNext();
		}

		dir.ListDirEnd();
		MyShipPath = myShipCount != 0 ? $"MyShip_{myShipCount}" : "MyShip";
	}
}
