using Godot;
using System;
using System.Collections.Generic;

public partial class Camera : Camera2D
{
    public float PanSpeed = 1.0f;
    public float ZoomSpeed = 0.1f;
    public float MinZoom = 0.1f;
    public float MaxZoom = 5.0f;
    public bool SmoothZoom = true;
    public bool SmoothFocus = false; // New variable to control smooth focus
    private bool isDragging = false;
    private Vector2 dragOrigin;
    private Vector2 targetZoom = new Vector2(1, 1);
    private float zoomDamping = 10.0f;
    private Body targetBody = null;
    // New variables to control focus zoom
    private Vector2 defaultFocusZoom = new Vector2(1f, 1f);
    private float focusTransitionSpeed = 3.0f; // Transition speed when focusing
    private float unfocusTransitionSpeed = 1.5f; // Transition speed when releasing focus
    private List<Ship> ships = new List<Ship>();

    // New variables for drag selection
    private bool isDragSelecting = false;
    private Vector2 selectionStart;
    private Vector2 selectionEnd;
    private Color selectionRectColor = new Color(0.2f, 0.8f, 1.0f, 0.3f); // Light blue with transparency

    public override void _Ready()
    {
        targetZoom = Zoom;
    }

    public override void _Process(double delta)
    {
        // Smooth zoom effect
        if (SmoothZoom)
        {
            Zoom = Zoom.Lerp(targetZoom, zoomDamping * (float)delta);
        }
        if (targetBody != null)
        {
            Focus(targetBody, (float)delta);
        }

        // Queue redraw when drag selecting to update the selection rectangle
        if (isDragSelecting)
        {
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        // Draw selection rectangle if drag selecting
        if (isDragSelecting)
        {
            // Convert screen to local coordinates for proper drawing
            Vector2 localStart = ToLocal(selectionStart);
            Vector2 localEnd = ToLocal(selectionEnd);
            Vector2 size = localEnd - localStart;

            DrawRect(new Rect2(localStart, size), selectionRectColor);

            // Draw border with slightly darker color
            Color borderColor = new Color(selectionRectColor.R, selectionRectColor.G, selectionRectColor.B, 0.8f);
            DrawRect(new Rect2(localStart, size), borderColor, false);
        }
    }

    public override void _Input(InputEvent @event)
    {
        // Handle mouse button events for panning
        if (@event is InputEventMouseButton mouseButton)
        {
            // Middle mouse button drag
            if (mouseButton.ButtonIndex == MouseButton.Right)
            {
                if (mouseButton.Pressed)
                {
                    // Start dragging
                    isDragging = true;
                    dragOrigin = mouseButton.Position;
                    ClearFocus();
                }
                else
                {
                    // Stop dragging
                    isDragging = false;
                }
            }
            // Mouse wheel zoom
            else if (mouseButton.ButtonIndex == MouseButton.WheelUp)
            {
                SmoothFocus = false; // Disable smooth zoom when using mouse wheel
                ZoomCamera(ZoomSpeed);
            }
            else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
            {
                SmoothFocus = false; // Disable smooth zoom when using mouse wheel
                ZoomCamera(-ZoomSpeed);
            }
            // Left mouse button for selection and movement
            else if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (mouseButton.Pressed)
                {
                    // FIRST, detect if we clicked on an object
                    Node2D hitObject = DetectClickedObject();

                    // If we didn't click on an object, start drag selection
                    if (hitObject == null)
                    {
                        // Store GLOBAL positions for selection
                        selectionStart = GetGlobalMousePosition();
                        selectionEnd = GetGlobalMousePosition();
                        isDragSelecting = true;
                    }
                    // If we have ships selected and didn't click on anything, prepare for movement
                    if (hitObject == null && ships.Count > 0)
                    {
                        // This is handled on release, not on press
                    }
                }
                else if (isDragSelecting) // Mouse button released after drag selecting
                {
                    // End drag selection
                    isDragSelecting = false;
                    QueueRedraw(); // Clear the selection rectangle

                    // Only perform selection if the rectangle has some size
                    if ((selectionStart - selectionEnd).Length() > 5)
                    {
                        // Select ships within the selection rectangle
                        SelectShipsInRectangle();
                    }
                    else if (ships.Count > 0)
                    {
                        // For small drags or clicks, move the ships
                        Vector2 baseTargetPosition = GetGlobalMousePosition();
                        MoveShipsInFormation(baseTargetPosition);
                    }
                }
            }
        }
        // Handle mouse motion for panning and selection rectangle
        else if (@event is InputEventMouseMotion mouseMotion)
        {
            if (isDragging)
            {
                // Calculate movement based on mouse motion and zoom level
                Vector2 movement = (dragOrigin - mouseMotion.Position) * PanSpeed / Zoom;
                Position += movement;
                dragOrigin = mouseMotion.Position;
            }
            else if (isDragSelecting)
            {
                // Update selection rectangle end point with GLOBAL position
                selectionEnd = GetGlobalMousePosition();
            }
        }
    }

    private void SelectShipsInRectangle()
    {
        // Since we're now storing global coordinates directly, 
        // we don't need to convert from screen to global
        Vector2 globalStart = selectionStart;
        Vector2 globalEnd = selectionEnd;

        // Create a rectangle with proper coordinates (min/max values for each axis)
        float minX = Math.Min(globalStart.X, globalEnd.X);
        float maxX = Math.Max(globalStart.X, globalEnd.X);
        float minY = Math.Min(globalStart.Y, globalEnd.Y);
        float maxY = Math.Max(globalStart.Y, globalEnd.Y);

        // Get all ships in the scene
        var allShips = GetTree().GetNodesInGroup("Ships");

        // Clear current selection
        foreach (Ship ship in ships)
        {
            if (ship != null)
            {
                ship.shipSelected = false;
            }
        }
        ships.Clear();

        // Check each ship
        foreach (Node node in allShips)
        {
            if (node is Ship ship && ship.isMine)
            {
                Vector2 shipPos = ship.GlobalPosition;

                // Check if the ship is within the selection rectangle
                if (shipPos.X >= minX && shipPos.X <= maxX &&
                    shipPos.Y >= minY && shipPos.Y <= maxY)
                {
                    ship.shipSelected = true;
                    ships.Add(ship);
                }
            }
        }

        GD.Print($"Selected {ships.Count} ships via drag selection");
    }

    private void MoveShipsInFormation(Vector2 baseTargetPosition)
    {
        // Calculate formation parameters
        int shipsPerRow = (int)Math.Ceiling(Math.Sqrt(ships.Count)); // Arrange in a square-ish grid
        float spacing = 100f; // Space between ships in the formation

        for (int i = 0; i < ships.Count; i++)
        {
            // Calculate grid position (row, column)
            int row = i / shipsPerRow;
            int col = i % shipsPerRow;

            // Calculate offset from center
            float xOffset = (col - (shipsPerRow - 1) / 2.0f) * spacing;
            float yOffset = (row - ((ships.Count - 1) / shipsPerRow) / 2.0f) * spacing;

            // Apply offset to create formation
            Vector2 offsetPosition = baseTargetPosition + new Vector2(xOffset, yOffset);

            ships[i].SetShipTarget(offsetPosition);
        }

        GD.Print($"Moving {ships.Count} ships in formation");
    }

    private void ZoomCamera(float zoomAmount)
    {
        if (SmoothZoom)
        {
            targetZoom += new Vector2(zoomAmount, zoomAmount);
            targetZoom = new Vector2(
                Mathf.Clamp(targetZoom.X, MinZoom, MaxZoom),
                Mathf.Clamp(targetZoom.Y, MinZoom, MaxZoom)
            );
        }
        else
        {
            Zoom += new Vector2(zoomAmount, zoomAmount);
            Zoom = new Vector2(
                Mathf.Clamp(Zoom.X, MinZoom, MaxZoom),
                Mathf.Clamp(Zoom.Y, MinZoom, MaxZoom)
            );
        }
    }

    private Node2D DetectClickedObject()
    {
        // Get mouse position in viewport
        Vector2 mousePos = GetViewport().GetMousePosition();

        // Convert to global position
        Vector2 globalPos = GetGlobalMousePosition();

        var spaceState = GetWorld2D().DirectSpaceState;

        // Cast rays in multiple directions to be more thorough
        var queryParams = new PhysicsRayQueryParameters2D();
        queryParams.From = globalPos;
        queryParams.To = globalPos; // Same position for point query
        queryParams.CollideWithAreas = true;
        queryParams.CollideWithBodies = true;
        queryParams.HitFromInside = true; // This is important to detect if we're inside an area

        // Try a shape cast instead of a ray cast
        var shape = new CircleShape2D();
        shape.Radius = 5.0f; // Small radius to detect nearby objects

        var shapeQuery = new PhysicsShapeQueryParameters2D();
        shapeQuery.Shape = shape;
        shapeQuery.Transform = new Transform2D(0, globalPos);
        shapeQuery.CollideWithAreas = true;
        shapeQuery.CollideWithBodies = true;

        var shapeResults = spaceState.IntersectShape(shapeQuery);

        // Use shape query results
        if (shapeResults.Count > 0)
        {
            foreach (var result in shapeResults)
            {
                GodotObject collider = result["collider"].As<GodotObject>();

                Node2D hitObject = collider as Node2D;
                if (hitObject != null)
                {
                    if (hitObject.GetParent()?.GetParent() is Body body)
                    {
                        if (targetBody != null && targetBody != body)
                        {
                            ClearFocus(); // Clear previous focus
                        }
                        targetBody = body;
                        if (targetBody is Star star)
                        {
                            star.starUI.SetUIVisible(true);
                        }
                        if (targetBody is Planet planet)
                        {
                            planet.planetUI.SetUIVisible(true);
                        }
                        SmoothFocus = true; // Enable smooth zoom when focusing on an object
                    }
                    if (hitObject.GetParent() is Ship ship)
                    {
                        if (!ship.isMine)
                        {
                            GD.Print("Clicked on: " + hitObject.Name);
                            return hitObject;

                        }
                        ship.shipSelected = !ship.shipSelected;
                        if (ship.shipSelected)
                        {
                            ships.Add(ship);
                            GD.Print("Clicked on: " + hitObject.Name);
                        }
                        else
                        {
                            ships.Remove(ship);
                        }
                    }
                    else
                    {
                        GD.Print("Clicked on: " + hitObject.Name);

                    }
                    return hitObject; // Return the clicked object
                }
            }
        }
        else
        {
            GD.Print("No objects found at click position");
        }
        return null; // No object clicked
    }

    public void Focus(Body Target, float deltaTime)
    {
        if (Target != null)
        {
            // Calculate smooth movement speed based on distance
            float distanceFactor = Position.DistanceTo(Target.GlobalPosition) * 0.01f;
            float smoothFactor = Mathf.Clamp(deltaTime * distanceFactor, 0.01f, 0.05f);

            // Apply the lerp and assign it back to Position
            Position = Position.Lerp(Target.GlobalPosition, smoothFactor);
            if (SmoothFocus)
            {
                // Smoothly transition the target zoom
                targetZoom = targetZoom.Lerp(defaultFocusZoom, deltaTime * focusTransitionSpeed);
            }

        }
    }

    public void ClearFocus()
    {
        if (targetBody is Star star)
        {
            star.starUI.SetUIVisible(false);
        }
        if (targetBody is Planet planet)
        {
            planet.planetUI.SetUIVisible(false);
        }
        targetBody = null;
    }
}