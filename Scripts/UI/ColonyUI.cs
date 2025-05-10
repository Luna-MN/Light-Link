using Godot;
using System;
using System.Linq;

public partial class ColonyUI : CanvasLayer // Change from Node2D to CanvasLayer
{
	// UI components
	private Panel colonyPanel;
	private Button closeButton;
	private VBoxContainer contentContainer;

	// Animation parameters
	private Tween tween;
	private bool isPanelVisible = false;
	private readonly float animationDuration = 0.3f;

	// Position constants
	private readonly float hiddenPositionX = -350f; // Hidden to the left
	private readonly float visiblePositionX = 0f;   // Visible at left edge
	public Colony colony;

	public ColonyUI(Colony colony)
	{
		this.colony = colony;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Set canvas layer
		Layer = 10; // Higher layer to ensure UI is on top

		var viewportSize = GetViewport().GetVisibleRect().Size;

		// Create the panel as left side panel
		colonyPanel = new Panel();
		colonyPanel.Name = "ColonyPanel";
		colonyPanel.Position = new Vector2(hiddenPositionX, 35);
		colonyPanel.Size = new Vector2(350, viewportSize.Y - 35);
		AddChild(colonyPanel);

		// Create close button (positioned at the top-right of the panel)
		closeButton = new Button();
		closeButton.Text = "X";
		closeButton.Position = new Vector2(310, 40);
		closeButton.Size = new Vector2(30, 30);
		closeButton.Pressed += TogglePanel;
		colonyPanel.AddChild(closeButton);

		// Create content container
		contentContainer = new VBoxContainer();
		contentContainer.Name = "ContentContainer";
		contentContainer.Position = new Vector2(20, 40);
		contentContainer.Size = new Vector2(310, viewportSize.Y - 100);
		contentContainer.SizeFlagsHorizontal = Control.SizeFlags.Fill;
		contentContainer.SizeFlagsVertical = Control.SizeFlags.Fill;
		colonyPanel.AddChild(contentContainer);

		// Add title label
		var titleLabel = new Label();
		titleLabel.Text = "Colony Management";
		titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		titleLabel.AddThemeFontSizeOverride("font_size", 24);
		titleLabel.CustomMinimumSize = new Vector2(0, 40);
		contentContainer.AddChild(titleLabel);

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
		contentContainer.AddChild(nameLabel);

		var nameValue = new Label();
		nameValue.Text = "Alpha Settlement";
		nameValue.AddThemeColorOverride("font_color", Colors.Yellow);
		contentContainer.AddChild(nameValue);

		// Population section
		var populationLabel = new Label();
		populationLabel.Text = "Population:";
		contentContainer.AddChild(populationLabel);

		var populationValue = new Label();
		populationValue.Text = colony.population.ToString();
		contentContainer.AddChild(populationValue);

		// Resources section
		var resourcesLabel = new Label();
		resourcesLabel.Text = "Resources:";
		contentContainer.AddChild(resourcesLabel);

		// Add some space
		var spacer = new Control();
		spacer.CustomMinimumSize = new Vector2(0, 20);
		contentContainer.AddChild(spacer);

		// Actions section header
		var actionsHeader = new Label();
		actionsHeader.Text = "Actions";
		actionsHeader.HorizontalAlignment = HorizontalAlignment.Center;
		actionsHeader.AddThemeFontSizeOverride("font_size", 18);
		contentContainer.AddChild(actionsHeader);

		// Action buttons
		var harvestButton = new Button();
		harvestButton.Text = "Harvest Resources";
		harvestButton.Pressed += () => OnHarvestPressed();
		contentContainer.AddChild(harvestButton);

		var buildButton = new Button();
		buildButton.Text = "Build Structure";
		buildButton.Pressed += () => OnBuildPressed();
		contentContainer.AddChild(buildButton);

		var researchButton = new Button();
		researchButton.Text = "Research Technology";
		researchButton.Pressed += () => OnResearchPressed();
		contentContainer.AddChild(researchButton);
	}

	// Toggle panel visibility
	private void TogglePanel()
	{
		SetUIVisible(!isPanelVisible);
	}

	public void SetUIVisible(bool visible)
	{
		if (isPanelVisible == visible)
			return; // Already in the requested state

		if (tween != null && tween.IsRunning())
			tween.Kill();

		tween = CreateTween();
		isPanelVisible = visible;

		// Slide in/out animation
		float targetX = isPanelVisible ? visiblePositionX : hiddenPositionX;
		tween.TweenProperty(colonyPanel, "position:x", targetX, animationDuration);

		// Enable/disable input processing based on visibility
		colonyPanel.ProcessMode = visible ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;

		// Always keep the panel visible to enable animations but block input when hidden
		colonyPanel.Visible = true;
	}
	public void UpdateText(string text, string value)
	{
		// Update the text of the label
		var label = contentContainer.GetChildren().OfType<Label>().FirstOrDefault(l => l.Text == text);
		if (label != null)
		{
			var updateLabel = contentContainer.GetChild(label.GetIndex() + 1) as Label;
			if (updateLabel != null)
			{
				updateLabel.Text = value;
			}
		}
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