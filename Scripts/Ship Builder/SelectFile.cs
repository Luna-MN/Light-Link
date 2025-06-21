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
	private List<ShipPreview> fileItems = new List<ShipPreview>();
	
	public string SelectedFilePath;
	public bool Save;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (!Directory.Exists("MyShips"))
		{
			Directory.CreateDirectory("res://MyShips");
		}
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
			// Skip directories and hidden files
			if (!dir.CurrentIsDir() && !fileName.StartsWith("."))
			{
				// Create file item
				GD.Print("Adding file: " + fileName);
				ShipPreview fileItemInstance = (ShipPreview)FileItem.Instantiate();
				FileList.AddChild(fileItemInstance);
				fileItemInstance.Text.Text = fileName;
				fileItemInstance.FilePath = path + "/" + fileName;
				fileItemInstance.Name = fileName;
				fileItemInstance.Select.ButtonDown += () =>
				{
					SelectedFilePath = fileItemInstance.FilePath;
				};
				fileItems.Add(fileItemInstance);
			}

			fileName = dir.GetNext();
		}

		dir.ListDirEnd();
	}
}
