using Godot;
using System;

public partial class Camera : Camera2D
{
    public float PanSpeed = 1.0f;
    public float ZoomSpeed = 0.1f;
    public float MinZoom = 0.5f;
    public float MaxZoom = 5.0f;
    public bool SmoothZoom = true;
    private bool isDragging = false;
    private Vector2 dragOrigin;
    private Vector2 targetZoom = new Vector2(1, 1);
    private float zoomDamping = 10.0f;

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
                ZoomCamera(ZoomSpeed);
            }
            else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
            {
                ZoomCamera(-ZoomSpeed);
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
}