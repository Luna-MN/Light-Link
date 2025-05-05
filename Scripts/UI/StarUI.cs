using Godot;
using System;

public partial class StarUI : Node2D
{
    // UI elements
    private Control rootContainer;
    private Control selectionBrackets;
    private Line2D connectionLine;
    private PanelContainer infoPanel;
    private Label starNameLabel;
    private Label starTypeLabel;
    private Label starPropertiesLabel;

    // Current star properties
    private StarProperties currentProperties;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        CreateUI();

        // Initially hide everything until a star is selected
        SetUIVisible(false);
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

    private void CreateInfoPanel()
    {
        // Create info panel in top-right
        infoPanel = new PanelContainer();
        infoPanel.Name = "InfoPanel";
        infoPanel.Position = new Vector2(100, 20); // Will be positioned dynamically
        infoPanel.Size = new Vector2(150, 100);

        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        infoPanel.AddChild(vbox);

        // Add labels for star info
        starNameLabel = new Label();
        starNameLabel.Name = "StarNameLabel";
        starNameLabel.Text = "Star Name";
        vbox.AddChild(starNameLabel);

        starTypeLabel = new Label();
        starTypeLabel.Name = "StarTypeLabel";
        starTypeLabel.Text = "Star Type";
        vbox.AddChild(starTypeLabel);

        starPropertiesLabel = new Label();
        starPropertiesLabel.Name = "StarPropertiesLabel";
        starPropertiesLabel.Text = "Properties...";
        vbox.AddChild(starPropertiesLabel);

        rootContainer.AddChild(infoPanel);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // You might want to update the UI position if the star moves
        if (currentProperties != null)
        {
            UpdateUIPosition();
        }
    }

    public void SetStarProperties(StarProperties properties)
    {
        currentProperties = properties;

        if (properties == null)
        {
            SetUIVisible(false);
            return;
        }

        // Update UI labels
        starNameLabel.Text = properties.Name ?? "Unknown Star";
        starTypeLabel.Text = $"Type: {properties.SType}";
        starPropertiesLabel.Text =
            $"Size: {properties.Radius}\n" +
            $"Temperature: {properties.Temperature}K";

        // Position UI around the star
        UpdateUIPosition();
    }

    private void UpdateUIPosition()
    {
        if (currentProperties == null)
            return;

        // The star itself is at (0,0) in local coordinates
        Vector2 starPosition = Vector2.Zero;
        float starRadius = currentProperties.Radius * 1.5f; // Smaller scale factor to avoid huge brackets

        // Position the selection brackets directly around the star
        selectionBrackets.Position = Vector2.Zero;
        selectionBrackets.Scale = Vector2.One * (starRadius / 10.0f); // Adjust scale to match star size

        // Position the info panel more reasonably
        infoPanel.Position = new Vector2(
            starRadius + 20, // To the right of the star
            -50 // Above the star
        );

        // Fix the connection line
        connectionLine.ClearPoints();
        // Start at edge of the star
        connectionLine.AddPoint(new Vector2(starRadius * 0.5f, -starRadius * 0.5f));
        // Connect to left side of info panel
        connectionLine.AddPoint(infoPanel.Position + new Vector2(0, 20));
    }

    public void SetUIVisible(bool visible)
    {
        if (rootContainer != null)
            rootContainer.Visible = visible;
    }
}