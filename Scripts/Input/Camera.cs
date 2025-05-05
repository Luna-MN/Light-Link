using Godot;
using System;

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
            if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
            {
                DetectClickedObject();
            }
        }

        // Handle mouse motion for panning
        if (@event is InputEventMouseMotion mouseMotion && isDragging)
        {
            // Calculate movement based on mouse motion and zoom level
            Vector2 movement = (dragOrigin - mouseMotion.Position) * PanSpeed / Zoom;
            Position += movement;
            dragOrigin = mouseMotion.Position;
        }

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
    private void DetectClickedObject()
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
                        targetBody = body;
                        if (targetBody is Star star)
                        {
                            star.starUI.SetUIVisible(true);
                        }
                        SmoothFocus = true; // Enable smooth zoom when focusing on an object
                    }
                }
            }
        }
        else
        {
            GD.Print("No objects found at click position");
        }
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
        targetBody = null;
    }
}