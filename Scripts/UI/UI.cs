using Godot;
using System;
[GlobalClass]
public partial class UI : CanvasLayer
{
	// UI element references
	private Control resourcePanel;
	private Label energyLabel;
	private Label metalLabel;
	private Label waterLabel;
	private Label fuelLabel;
	private Control pauseMenu;

	// Colony state
	private int energy = 100;
	private int metal = 50;
	private float water = 75.0f;
	private int fuel = 10;
	private bool isPaused = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Create UI elements at runtime
		CreateResourcePanel();
		CreatePauseMenu();

		// Initialize UI
		UpdateResourceDisplay();
		pauseMenu.Visible = false;

		// Ensure the UI keeps processing when the game is paused
		ConfigurePauseProcessing();
	}

	private void CreateResourcePanel()
	{
		// Create a panel for resources along the top
		resourcePanel = new PanelContainer();
		resourcePanel.Name = "ResourcePanel";
		resourcePanel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
		resourcePanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

		// Create a horizontal container for resource indicators
		var hboxContainer = new HBoxContainer();
		hboxContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		hboxContainer.AddThemeConstantOverride("separation", 20);
		hboxContainer.Size = new Vector2(GetViewport().GetVisibleRect().Size.X, 40);

		// Create Food indicator with icon
		var foodContainer = new HBoxContainer();
		var foodIcon = new TextureRect();
		foodIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		foodIcon.CustomMinimumSize = new Vector2(24, 24);
		foodContainer.AddChild(foodIcon);

		energyLabel = new Label();
		energyLabel.Text = "Energy:";
		foodContainer.AddChild(energyLabel);
		hboxContainer.AddChild(foodContainer);

		// Create Power indicator with icon
		var powerContainer = new HBoxContainer();
		var powerIcon = new TextureRect();
		powerIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		powerIcon.CustomMinimumSize = new Vector2(24, 24);
		powerContainer.AddChild(powerIcon);

		metalLabel = new Label();
		metalLabel.Text = "Metal:";
		powerContainer.AddChild(metalLabel);
		hboxContainer.AddChild(powerContainer);

		// Create Happiness indicator with icon
		var happinessContainer = new HBoxContainer();
		var happinessIcon = new TextureRect();
		happinessIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		happinessIcon.CustomMinimumSize = new Vector2(24, 24);
		happinessContainer.AddChild(happinessIcon);

		waterLabel = new Label();
		waterLabel.Text = "Water:";
		happinessContainer.AddChild(waterLabel);
		hboxContainer.AddChild(happinessContainer);

		// Create Population indicator with icon
		var populationContainer = new HBoxContainer();
		var populationIcon = new TextureRect();
		populationIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		populationIcon.CustomMinimumSize = new Vector2(24, 24);
		populationContainer.AddChild(populationIcon);

		fuelLabel = new Label();
		fuelLabel.Text = "Fuel:";
		populationContainer.AddChild(fuelLabel);
		hboxContainer.AddChild(populationContainer);

		resourcePanel.AddChild(hboxContainer);
		AddChild(resourcePanel);
	}

	private void CreatePauseMenu()
	{
		// Create main pause menu container
		pauseMenu = new Control();
		pauseMenu.Name = "PauseMenu";
		pauseMenu.SetAnchorsPreset(Control.LayoutPreset.FullRect);

		// Create semi-transparent background
		var panel = new Panel();
		panel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		panel.SelfModulate = new Color(0, 0, 0, 0.7f);
		pauseMenu.AddChild(panel);

		// Create center container for buttons
		var centerContainer = new VBoxContainer();
		centerContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
		centerContainer.Size = new Vector2(200, 150);
		centerContainer.Position -= centerContainer.Size / 2; // Center it
		centerContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		centerContainer.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
		pauseMenu.AddChild(centerContainer);

		// Add pause menu title
		var titleLabel = new Label();
		titleLabel.Text = "Colony Management";
		titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		centerContainer.AddChild(titleLabel);

		// Add a small spacer
		var spacer = new Control();
		spacer.CustomMinimumSize = new Vector2(0, 20);
		centerContainer.AddChild(spacer);

		// Add resume button
		var resumeButton = new Button();
		resumeButton.Text = "Resume Colony";
		resumeButton.Pressed += OnResumePressed;
		centerContainer.AddChild(resumeButton);

		// Add quit button
		var quitButton = new Button();
		quitButton.Text = "Abandon Colony";
		quitButton.Pressed += OnQuitPressed;
		centerContainer.AddChild(quitButton);

		AddChild(pauseMenu);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Check for pause button press
		if (Input.IsActionJustPressed("ui_cancel")) // Checks for Escape key press
		{
			// Toggle pause state
			TogglePause();
		}
	}

	// Resource update methods
	public void UpdateEnergy(int amount)
	{
		energy += amount;
		UpdateResourceDisplay();
	}

	public void UpdateMetal(int amount)
	{
		metal += amount;
		UpdateResourceDisplay();
	}

	public void UpdateWater(float amount)
	{
		water += amount;
		UpdateResourceDisplay();
	}

	public void UpdateFuel(int amount)
	{
		fuel += amount;
		UpdateResourceDisplay();
	}

	private void UpdateResourceDisplay()
	{
		if (energyLabel != null)
			energyLabel.Text = $"Energy: {energy}";

		if (metalLabel != null)
			metalLabel.Text = $"Metal: {metal}";

		if (waterLabel != null)
			waterLabel.Text = $"Water: {water}";

		if (fuelLabel != null)
			fuelLabel.Text = $"Fuel: {fuel}";
	}

	private void TogglePause()
	{
		isPaused = !isPaused;

		// Important: Don't pause the UI node or its children
		// Otherwise the buttons won't respond
		GetTree().Paused = isPaused;

		// Make sure the pause menu processes input when visible
		if (pauseMenu != null)
		{
			pauseMenu.Visible = isPaused;
			pauseMenu.ProcessMode = isPaused ? ProcessModeEnum.Always : ProcessModeEnum.Inherit;
		}

		GD.Print($"Colony Management Paused: {isPaused}");
	}

	// Called from the resume button in the pause menu
	public void OnResumePressed()
	{
		TogglePause();
	}

	// Called from the quit button in the pause menu
	public void OnQuitPressed()
	{
		GetTree().Quit();
	}

	// Call this in _Ready to ensure the UI keeps processing when game is paused
	private void ConfigurePauseProcessing()
	{
		// Make sure the UI itself continues to process when the game is paused
		this.ProcessMode = ProcessModeEnum.Always;
	}
}