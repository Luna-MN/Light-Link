using Godot;
using System;
using System.Collections.Generic;

public partial class ContextMenu : Node2D
{
    // The visual container for our menu
    protected PopupMenu popup;

    // Dictionary to map menu item IDs to their handlers
    protected Dictionary<int, Action> actionMap = new Dictionary<int, Action>();

    // Keep track of the next available item ID
    protected int nextId = 0;

    // Signal emitted when an item is selected
    [Signal]
    public delegate void ItemSelectedEventHandler(int id, string text);

    public override void _Ready()
    {
        // Create the popup menu
        popup = new PopupMenu();
        AddChild(popup);
        AddToGroup("ContextMenus");

        // Set properties to allow proper menu interaction
        popup.Exclusive = false;

        popup.IdPressed += OnIdPressed;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        // Only check for input when the popup is visible
        if (popup.Visible)
        {
            Vector2 mousePos = GetViewport().GetMousePosition();
            Rect2 rect = new Rect2(popup.Position, popup.Size);

            if (rect.HasPoint(mousePos))
            {
                // When mouse is over menu, make sure it can receive input
                GetViewport().SetInputAsHandled();
            }
        }
    }

    // Show the menu at the mouse position
    public void ShowAtMouse()
    {
        Vector2 mousePosition = GetViewport().GetMousePosition();
        ShowAt(mousePosition);
    }

    // Show at a specific screen position
    public void ShowAt(Vector2 position)
    {
        popup.Position = new Vector2I((int)position.X, (int)position.Y);
        popup.Exclusive = false; // Allow input to pass through
        popup.Popup();
    }

    // Add an item to the menu
    public int AddItem(string text, Action callback = null)
    {
        int id = nextId++;
        popup.AddItem(text, id);

        if (callback != null)
        {
            actionMap[id] = callback;
        }

        return id;
    }

    // Add a disabled (grayed out) item to the menu
    public int AddDisabledItem(string text, string disabledReason = null)
    {
        int id = AddItem(text);
        int idx = popup.GetItemIndex(id);
        SetItemDisabled(idx, true);

        if (!string.IsNullOrEmpty(disabledReason))
        {
            popup.SetItemTooltip(idx, disabledReason);
        }

        return id;
    }

    // Add a separator line
    public void AddSeparator()
    {
        popup.AddSeparator();
    }

    // Clear all items
    public void ClearItems()
    {
        popup.Clear();
        actionMap.Clear();
        nextId = 0;
    }

    // Handler for when an item is selected
    private void OnIdPressed(long id)
    {
        int itemId = (int)id;
        int index = popup.GetItemIndex(itemId);
        string text = popup.GetItemText(index);

        // Emit the signal
        EmitSignal(SignalName.ItemSelected, itemId, text);

        // Execute the callback if one exists
        if (actionMap.ContainsKey(itemId))
        {
            actionMap[itemId]?.Invoke();
        }
    }

    public void Hide()
    {
        popup.Hide();
    }

    // Close the menu when clicking outside
    public override void _UnhandledInput(InputEvent @event)
    {
        if (popup.Visible && @event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            Vector2 mousePos = GetViewport().GetMousePosition();
            Rect2 rect = new Rect2(popup.Position, popup.Size);

            if (!rect.HasPoint(mousePos))
            {
                Hide();
                // GetViewport().SetInputAsHandled();
            }
        }
    }

    // Set item disabled state
    public void SetItemDisabled(int idx, bool disabled)
    {
        popup.SetItemDisabled(idx, disabled);
    }

    // Set item checked state
    public void SetItemChecked(int idx, bool isChecked)
    {
        popup.SetItemChecked(idx, isChecked);
    }

    // Add a submenu item
    public int AddSubmenuItem(string text, string submenu)
    {
        int id = nextId++;
        popup.AddSubmenuItem(text, submenu, id);
        return id;
    }

    // Check if the menu is currently visible
    public bool IsVisible()
    {
        return popup.Visible;
    }

    // Get the underlying PopupMenu for custom configurations
    public PopupMenu GetPopup()
    {
        return popup;
    }
}