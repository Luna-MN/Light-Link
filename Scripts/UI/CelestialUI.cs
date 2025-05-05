using Godot;
using System;

public partial class CelestialUI : Node2D
{
	public Control rootContainer;
	public Control selectionBrackets;
	public Line2D connectionLine;
	public Panel infoPanel; // Changed from PanelContainer to Control
	private bool isDraggingInfoPanel = false;
	private Vector2 dragStartPosition;

	// Called when the node enters the scene tree for the first time.
	public CelestialUI()
	{
		CreateUI();
		// Initially hide everything until a star is selected
		SetUIVisible(false);
	}
	public override void _Ready()
	{
		ZIndex = 1000; // Set a high ZIndex to ensure UI is on top of other elements
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void CreateUI()
	{
		// Root container for all UI elements
		rootContainer = new Control();
		rootContainer.Name = "StarUIRoot";

		// Don't set full rect anchors - this is causing the issue
		// rootContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);

		// Instead, position at the origin with appropriate size
		rootContainer.Position = Vector2.Zero;
		rootContainer.Size = new Vector2(300, 200);

		AddChild(rootContainer);

		// Create selection brackets (the corner markers)
		CreateSelectionBrackets();

		// Create connecting line from top-right bracket to info panel
		connectionLine = new Line2D();
		connectionLine.Name = "ConnectionLine";
		connectionLine.Width = 1.5f;
		connectionLine.DefaultColor = Colors.White;
		rootContainer.AddChild(connectionLine);

		// Create info panel
		CreateInfoPanel();
	}
	private void CreateInfoPanel()
	{
		infoPanel = new Panel();
		infoPanel.Name = "InfoPanel";

		// Explicitly set anchors to top-left to allow free movement
		infoPanel.AnchorLeft = 0;
		infoPanel.AnchorTop = 0;
		infoPanel.AnchorRight = 0;
		infoPanel.AnchorBottom = 0;

		infoPanel.Position = new Vector2(100, 10);
		infoPanel.Size = new Vector2(250, 10);

		// IMPORTANT: Set MouseFilter to Pass to allow proper event propagation
		infoPanel.MouseFilter = Control.MouseFilterEnum.Pass;

		var panelBackground = new Panel();
		panelBackground.Name = "Background";
		panelBackground.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		panelBackground.MouseFilter = Control.MouseFilterEnum.Ignore;
		infoPanel.AddChild(panelBackground);

		var vbox = new VBoxContainer();
		vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		vbox.Name = "Content";
		vbox.AddThemeConstantOverride("separation", 4);
		panelBackground.AddChild(vbox);

		rootContainer.AddChild(infoPanel);

		infoPanel.QueueRedraw();
	}
	private void CreateSelectionBrackets()
	{
		selectionBrackets = new Control();
		selectionBrackets.Name = "SelectionBrackets";
		rootContainer.AddChild(selectionBrackets);

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
	public void SetUIVisible(bool visible)
	{
		if (rootContainer != null)
			rootContainer.Visible = visible;
	}
	public Label AddProperty(string name, object value, string Units = "")
	{
		infoPanel.Size += new Vector2(0, 30); // Increase the height of the info area
		Label propertyLabel = new Label();
		if (value is float f)
		{
			value = Mathf.Round(f * 100) / 100;
		}
		propertyLabel.Text = $"{name}: {value}";
		propertyLabel.Name = name + "Label";
		if (Units != "")
		{
			propertyLabel.Text += " " + Units;

		}
		if (infoPanel != null)
		{
			var vbox = infoPanel.GetNode<Panel>("Background").GetNode<VBoxContainer>("Content");
			vbox.AddChild(propertyLabel);
		}
		return propertyLabel;
	}

}
