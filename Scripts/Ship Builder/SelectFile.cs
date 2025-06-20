using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SelectFile : Control
{
	[Export] public BoxContainer FileList;
	[Export] public Button Browse, Select;
	[Export] public PackedScene FileItem;
	private List<ShipPreview> fileItems = new List<ShipPreview>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void FindFiles(string path)
	{
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
				ShipPreview fileItemInstance = (ShipPreview)FileItem.Instantiate();
				FileList.AddChild(fileItemInstance);
				fileItemInstance.Text.Text = fileName;
				fileItemInstance.Name = fileName;
				fileItems.Add(fileItemInstance);
				
			}

			fileName = dir.GetNext();
		}

		dir.ListDirEnd();
	}
}
