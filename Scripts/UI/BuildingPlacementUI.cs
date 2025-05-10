using Godot;
using System;
using System.Collections.Generic;

public partial class BuildingPlacementUI : CanvasLayer
{
    // UI components
    private Panel buildingPanel;
    private Button closeButton;
    private GridContainer buildingGrid;
    private Label titleLabel;

    // Selected building
    private string selectedBuildingType;
    public bool isPlacementMode = false;

    // Building types with icons and descriptions
    private readonly Dictionary<string, BuildingInfo> buildingTypes = new Dictionary<string, BuildingInfo>
    {
        { "Gas Skimmer", new BuildingInfo { Description = "", Cost = 10 } },
        { "Metal Processor", new BuildingInfo { Description = "", Cost = 15 } },
        { "Asteroid Miner", new BuildingInfo { Description = "", Cost = 20 } },
        { "Research Lab", new BuildingInfo { Description = "", Cost = 30 } },
        { "Power Plant", new BuildingInfo { Description = "", Cost = 25 } }
    };

    // Animation parameters
    private Tween tween;
    private bool isPanelVisible = false;
    private readonly float animationDuration = 0.3f;

    // Position constants
    private readonly float hiddenPositionY = 800f; // Hidden below screen
    private float visiblePositionY; // Will be calculated based on viewport height

    // Signals
    [Signal]
    public delegate void BuildingSelectedEventHandler(string buildingType);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // Set canvas layer
        Layer = 11;

        var viewportSize = GetViewport().GetVisibleRect().Size;

        // Calculate the visible position to be at the bottom of the screen
        visiblePositionY = viewportSize.Y - 210;

        // Create the panel
        buildingPanel = new Panel();
        buildingPanel.Name = "BuildingPanel";

        // Use 80% of viewport width and center it
        float panelWidth = viewportSize.X * 0.8f;
        float panelOffsetX = viewportSize.X * 0.1f; // 10% padding on each side

        buildingPanel.Position = new Vector2(panelOffsetX, hiddenPositionY);
        buildingPanel.Size = new Vector2(panelWidth, 210);
        AddChild(buildingPanel);

        // Create close button
        closeButton = new Button();
        closeButton.Text = "X";
        closeButton.Position = new Vector2(buildingPanel.Size.X - 40, 10);
        closeButton.Size = new Vector2(30, 30);
        closeButton.Pressed += TogglePanel;
        buildingPanel.AddChild(closeButton);

        // Center the title on the panel
        titleLabel = new Label();
        titleLabel.Text = "Select Building";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 20);
        titleLabel.Position = new Vector2(buildingPanel.Size.X / 2 - 50, 15);
        titleLabel.Size = new Vector2(100, 30);
        buildingPanel.AddChild(titleLabel);

        // Create grid container
        buildingGrid = new GridContainer();
        buildingGrid.Name = "BuildingGrid";
        buildingGrid.Position = new Vector2(buildingPanel.Size.X * 0.05f, 50);
        buildingGrid.Size = new Vector2(buildingPanel.Size.X * 0.9f, 150);
        buildingGrid.Columns = 5;
        buildingPanel.AddChild(buildingGrid);

        // Add building options
        CreateBuildingOptions();

        GetTree().Root.SizeChanged += UpdateUILocation;

        // Set initial visibility
        SetUIVisible(true);
    }

    public void UpdateUILocation()
    {
        var viewportSize = GetViewport().GetVisibleRect().Size;

        // Update positions based on current viewport
        visiblePositionY = viewportSize.Y - 210; // More space for buttons

        // Calculate new panel width and position
        float panelWidth = viewportSize.X * 0.8f;
        float panelOffsetX = viewportSize.X * 0.1f;

        // Update panel size and position
        buildingPanel.Size = new Vector2(panelWidth, 210);

        // Update position while maintaining current visibility state
        if (isPanelVisible)
        {
            buildingPanel.Position = new Vector2(panelOffsetX, visiblePositionY);
        }
        else
        {
            // When hidden, position below the screen
            float hiddenY = viewportSize.Y + 50;
            buildingPanel.Position = new Vector2(panelOffsetX, hiddenY);
        }

        // Update child elements
        closeButton.Position = new Vector2(buildingPanel.Size.X - 40, 10);
        titleLabel.Position = new Vector2(buildingPanel.Size.X / 2 - 50, 15);

        // Update grid size and position
        buildingGrid.Position = new Vector2(buildingPanel.Size.X * 0.05f, 50);
        buildingGrid.Size = new Vector2(buildingPanel.Size.X * 0.9f, 150);
    }

    // Create building selection buttons
    private void CreateBuildingOptions()
    {
        // Clear any existing children first
        foreach (Node child in buildingGrid.GetChildren())
        {
            buildingGrid.RemoveChild(child);
            child.QueueFree();
        }

        foreach (var building in buildingTypes)
        {
            // Create a button as the main container
            var buildingButton = new Button();
            buildingButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            buildingButton.CustomMinimumSize = new Vector2(180, 170);
            buildingButton.FocusMode = Control.FocusModeEnum.None;
            buildingButton.Pressed += () => OnBuildingSelected(building.Key);

            // Center align button content
            buildingButton.Alignment = HorizontalAlignment.Center;
            // Use a CenterContainer to force better centering
            var centerContainer = new CenterContainer();
            centerContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            centerContainer.LayoutMode = 1;
            centerContainer.AnchorsPreset = 15;
            centerContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            centerContainer.MouseFilter = Control.MouseFilterEnum.Ignore;
            buildingButton.AddChild(centerContainer);

            // Add a VBox inside the center container to organize content
            var contentContainer = new VBoxContainer();
            contentContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
            contentContainer.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
            contentContainer.MouseFilter = Control.MouseFilterEnum.Ignore;
            // Add custom margins to improve spacing
            contentContainer.AddThemeConstantOverride("separation", 10);
            centerContainer.AddChild(contentContainer);

            // Building icon (placeholder)
            var iconPanel = new Panel();
            iconPanel.CustomMinimumSize = new Vector2(60, 60);
            iconPanel.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
            iconPanel.MouseFilter = Control.MouseFilterEnum.Ignore;
            contentContainer.AddChild(iconPanel);

            // Label for building name
            var nameLabel = new Label();
            nameLabel.Text = building.Key;
            nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            nameLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
            contentContainer.AddChild(nameLabel);

            // Label for cost
            var costLabel = new Label();
            costLabel.Text = $"Cost: {building.Value.Cost}";
            costLabel.HorizontalAlignment = HorizontalAlignment.Center;
            costLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
            contentContainer.AddChild(costLabel);

            // Add the button to the grid
            buildingGrid.AddChild(buildingButton);
        }
    }
    // Toggle panel visibility
    public void TogglePanel()
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

        // Calculate the target position
        float targetY;
        if (isPanelVisible)
        {
            targetY = visiblePositionY;
        }
        else
        {
            // Use dynamic hidden position based on current viewport size
            targetY = GetViewport().GetVisibleRect().Size.Y + 50;
        }

        tween.TweenProperty(buildingPanel, "position:y", targetY, animationDuration);

        // Enable/disable input processing based on visibility
        buildingPanel.ProcessMode = visible ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;

        // Always keep the panel visible to enable animations but block input when hidden
        buildingPanel.Visible = true;

        // Reset placement mode when hiding
        if (!visible)
            isPlacementMode = false;
    }

    // Handler for building selection
    private void OnBuildingSelected(string buildingType)
    {
        selectedBuildingType = buildingType;
        isPlacementMode = true;

        GD.Print($"Selected building: {buildingType}");

        // Emit signal that a building was selected
        EmitSignal(SignalName.BuildingSelected, buildingType);

    }

    // Building info class to store building details
    public class BuildingInfo
    {
        public string Description { get; set; }
        public int Cost { get; set; }
    }
}