using Godot;
using System;

public partial class ColonyUI : CanvasLayer // Change from Node2D to CanvasLayer
{
	// UI components
	private Panel _colonyPanel;
	private Button _closeButton;
	private VBoxContainer _contentContainer;

	// Animation parameters
	private Tween _tween;
	private bool _isPanelVisible = false;
	private readonly float _animationDuration = 0.3f;

	// Position constants
	private readonly float _hiddenPositionX = -350f; // Hidden to the left
	private readonly float _visiblePositionX = 0f;   // Visible at left edge

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Set canvas layer
		Layer = 10; // Higher layer to ensure UI is on top

		var viewportSize = GetViewport().GetVisibleRect().Size;

		// Create the panel as left side panel
		_colonyPanel = new Panel();
		_colonyPanel.Name = "ColonyPanel";
		_colonyPanel.Position = new Vector2(_hiddenPositionX, 0);
		_colonyPanel.Size = new Vector2(350, viewportSize.Y);
		AddChild(_colonyPanel);

		// Create close button (positioned at the top-right of the panel)
		_closeButton = new Button();
		_closeButton.Text = "X";
		_closeButton.Position = new Vector2(310, 10);
		_closeButton.Size = new Vector2(30, 30);
		_closeButton.Pressed += TogglePanel;
		_colonyPanel.AddChild(_closeButton);

		// Create content container
		_contentContainer = new VBoxContainer();
		_contentContainer.Name = "ContentContainer";
		_contentContainer.Position = new Vector2(20, 20);
		_contentContainer.Size = new Vector2(310, viewportSize.Y - 40);
		_contentContainer.SizeFlagsHorizontal = Control.SizeFlags.Fill;
		_contentContainer.SizeFlagsVertical = Control.SizeFlags.Fill;
		_colonyPanel.AddChild(_contentContainer);

		// Add title label
		var titleLabel = new Label();
		titleLabel.Text = "Colony Management";
		titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		titleLabel.AddThemeFontSizeOverride("font_size", 24);
		titleLabel.CustomMinimumSize = new Vector2(0, 40);
		_contentContainer.AddChild(titleLabel);

		// Add sample UI elements
		CreateColonyUI();

		// Set initial visibility
		SetUIVisible(false);
	}

	// Create the colony UI elements
	private void CreateColonyUI()
	{
		// Colony name section
		var nameLabel = new Label();
		nameLabel.Text = "Colony Name:";
		_contentContainer.AddChild(nameLabel);

		var nameValue = new Label();
		nameValue.Text = "Alpha Settlement";
		nameValue.AddThemeColorOverride("font_color", Colors.Yellow);
		_contentContainer.AddChild(nameValue);

		// Population section
		var populationLabel = new Label();
		populationLabel.Text = "Population:";
		_contentContainer.AddChild(populationLabel);

		var populationValue = new Label();
		populationValue.Text = "250";
		_contentContainer.AddChild(populationValue);

		// Resources section
		var resourcesLabel = new Label();
		resourcesLabel.Text = "Resources:";
		_contentContainer.AddChild(resourcesLabel);

		// Add some space
		var spacer = new Control();
		spacer.CustomMinimumSize = new Vector2(0, 20);
		_contentContainer.AddChild(spacer);

		// Actions section header
		var actionsHeader = new Label();
		actionsHeader.Text = "Actions";
		actionsHeader.HorizontalAlignment = HorizontalAlignment.Center;
		actionsHeader.AddThemeFontSizeOverride("font_size", 18);
		_contentContainer.AddChild(actionsHeader);

		// Action buttons
		var harvestButton = new Button();
		harvestButton.Text = "Harvest Resources";
		harvestButton.Pressed += () => OnHarvestPressed();
		_contentContainer.AddChild(harvestButton);

		var buildButton = new Button();
		buildButton.Text = "Build Structure";
		buildButton.Pressed += () => OnBuildPressed();
		_contentContainer.AddChild(buildButton);

		var researchButton = new Button();
		researchButton.Text = "Research Technology";
		researchButton.Pressed += () => OnResearchPressed();
		_contentContainer.AddChild(researchButton);
	}

	// Toggle panel visibility
	private void TogglePanel()
	{
		SetUIVisible(!_isPanelVisible);
	}

	public void SetUIVisible(bool visible)
	{
		if (_isPanelVisible == visible)
			return; // Already in the requested state

		if (_tween != null && _tween.IsRunning())
			_tween.Kill();

		_tween = CreateTween();
		_isPanelVisible = visible;

		// Slide in/out animation
		float targetX = _isPanelVisible ? _visiblePositionX : _hiddenPositionX;
		_tween.TweenProperty(_colonyPanel, "position:x", targetX, _animationDuration);

		// Enable/disable input processing based on visibility
		_colonyPanel.ProcessMode = visible ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;

		// Always keep the panel visible to enable animations but block input when hidden
		_colonyPanel.Visible = true;
	}

	// Example action handlers
	private void OnHarvestPressed()
	{
		GD.Print("Harvesting resources from colony");
		// Implement colony resource harvesting logic
	}

	private void OnBuildPressed()
	{
		GD.Print("Opening build menu");
		// Implement building functionality
	}

	private void OnResearchPressed()
	{
		GD.Print("Opening research menu");
		// Implement research functionality
	}
}