using Godot;
using System;

public partial class ResourceShip : Ship
{
    public float MaxRange = 1000f;
    public override void _Ready()
    {
        base._Ready();
        Mesh = new ResourceShipMesh();
        Scale = new Vector2(10, 10);
        AddChild(Mesh);
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        // Add any additional processing logic here
        Resource closestResource = FindNearbyResources();
        if (closestResource != null && Position.DistanceTo(closestResource.Position) < MaxRange)
        {
            targetPosition = closestResource.Position;
        }
        if (closestResource != null && Position.DistanceTo(closestResource.Position) < 10f)
        {
            AttachResource(closestResource);
        }
    }
    public Resource FindNearbyResources()
    {
        Vector2 ClosestResource = new Vector2(float.MaxValue, float.MaxValue);
        Resource closestResource = null;
        // Implement logic to find nearby resources
        foreach (Resource resource in GetTree().GetNodesInGroup("Resources"))
        {
            float distance = resource.Position.DistanceTo(Position);
            if (distance < ClosestResource.DistanceTo(Position))
            {
                ClosestResource = resource.Position;
                closestResource = resource;
            }
        }
        return closestResource;
    }
    public void AttachResource(Resource resource)
    {
        if (resource != null)
        {
            // Implement logic to attach the resource to the ship
            GD.Print("Resource attached: " + resource.Name);
        }
    }
}