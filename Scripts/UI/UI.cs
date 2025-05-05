using Godot;
using System;

public partial class UI : CanvasLayer
{
	// UI element references
	private Control resourcePanel;
	private Label foodLabel;
	private Label powerLabel;
	private Label happinessLabel;
	private Label populationLabel;
	private Control pauseMenu;

	// Colony state
	private int food = 100;
	private int power = 50;
	private float happiness = 75.0f;
	private int population = 10;
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
		foodIcon.Texture = ResourceLoader.Load<Texture2D>("res://assets/ui/food_icon.png"); // Create this icon
		foodIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		foodIcon.CustomMinimumSize = new Vector2(24, 24);
		foodContainer.AddChild(foodIcon);

		foodLabel = new Label();
		foodLabel.Text = "Food: 100";
		foodContainer.AddChild(foodLabel);
		hboxContainer.AddChild(foodContainer);

		// Create Power indicator with icon
		var powerContainer = new HBoxContainer();
		var powerIcon = new TextureRect();
		powerIcon.Texture = ResourceLoader.Load<Texture2D>("res://assets/ui/power_icon.png"); // Create this icon
		powerIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		powerIcon.CustomMinimumSize = new Vector2(24, 24);
		powerContainer.AddChild(powerIcon);

		powerLabel = new Label();
		powerLabel.Text = "Power: 50";
		powerContainer.AddChild(powerLabel);
		hboxContainer.AddChild(powerContainer);

		// Create Happiness indicator with icon
		var happinessContainer = new HBoxContainer();
		var happinessIcon = new TextureRect();
		happinessIcon.Texture = ResourceLoader.Load<Texture2D>("res://assets/ui/happiness_icon.png"); // Create this icon
		happinessIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		happinessIcon.CustomMinimumSize = new Vector2(24, 24);
		happinessContainer.AddChild(happinessIcon);

		happinessLabel = new Label();
		happinessLabel.Text = "Happiness: 75%";
		happinessContainer.AddChild(happinessLabel);
		hboxContainer.AddChild(happinessContainer);

		// Create Population indicator with icon
		var populationContainer = new HBoxContainer();
		var populationIcon = new TextureRect();
		populationIcon.Texture = ResourceLoader.Load<Texture2D>("res://assets/ui/population_icon.png"); // Create this icon
		populationIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		populationIcon.CustomMinimumSize = new Vector2(24, 24);
		populationContainer.AddChild(populationIcon);

		populationLabel = new Label();
		populationLabel.Text = "Population: 10";
		populationContainer.AddChild(populationLabel);
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
	public void UpdateFood(int amount)
	{
		food += amount;
		UpdateResourceDisplay();
	}

	public void UpdatePower(int amount)
	{
		power += amount;
		UpdateResourceDisplay();
	}

	public void UpdateHappiness(float amount)
	{
		happiness = Mathf.Clamp(happiness + amount, 0, 100);
		UpdateResourceDisplay();
	}

	public void UpdatePopulation(int amount)
	{
		population += amount;
		UpdateResourceDisplay();
	}

	private void UpdateResourceDisplay()
	{
		if (foodLabel != null)
			foodLabel.Text = $"Food: {food}";

		if (powerLabel != null)
			powerLabel.Text = $"Power: {power}";

		if (happinessLabel != null)
			happinessLabel.Text = $"Happiness: {happiness:F0}%";

		if (populationLabel != null)
			populationLabel.Text = $"Population: {population}";
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