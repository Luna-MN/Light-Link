using Godot;
using System;

public partial class ResourceShip : Ship
{
    public float MaxRange = 1000f;
    public float PickupRange = 100f;
    public Vector2 offset = new Vector2(0, 0);
    public Resource closestResource = null;
    public Building closestProcessor = null;
    public Line2D tractorBeam;
    public override void _Ready()
    {
        base._Ready();
        Mesh = new ResourceShipMesh();
        Mesh.Scale = 10f;
        AddChild(Mesh);
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        // Add any additional processing logic here
        resourceAttachment();
        // Update tractor beam if it exists
        if (tractorBeam != null && IsInstanceValid(closestResource))
        {
            tractorBeam.Points = new Vector2[]
            {
                ToLocal(GlobalPosition),
                ToLocal(closestResource.GlobalPosition)
            };
        }
        if (closestResource?.isAttached == true && ((path.Count > 0 && path[0] != closestProcessor.GlobalPosition) || path.Count == 0))
        {
            MoveToResourceProcessing();
        }
        else if (closestProcessor != null && closestResource?.isAttached == true && GlobalPosition.DistanceSquaredTo(closestProcessor.GlobalPosition) < 100f)
        {
            // If the resource is attached and close to the processor, process it
            GD.Print("Processing resource at processor: " + closestProcessor.Name);
            closestResource.ProcessResource((ResourcePlace)closestProcessor);
            closestResource = null; // Clear the resource after processing
        }

    }
    public void resourceAttachment()
    {
        if (closestResource == null)
        {
            closestResource = FindNearbyResources();
        }
        if (closestResource != null && Position.DistanceTo(closestResource.Position) < MaxRange && closestResource.resourceShip == null)
        {
            path.Add(closestResource.GlobalPosition);
            closestResource.resourceShip = this;
        }
        if (closestResource?.resourceShip == this && closestResource.isAttached == false)
        {
            AttachResource(closestResource);
        }
        else if (closestResource?.resourceShip != this)
        {
            closestResource = null;
        }
    }
    public void MoveToResourceProcessing()
    {
        closestProcessor = FindClosestProcessor();
        if (closestProcessor != null)
        {
            path.Add(closestProcessor.GlobalPosition);
            GD.Print("Moving to processor: " + closestProcessor);
        }
        else
        {
            GD.Print("No processor found.");
        }
    }
    public Building FindClosestProcessor()
    {
        // Implement logic to find the closest processor
        // This could be a method that checks for nearby processors and returns the closest one
        // For now, we'll just print a message
        Vector2 closestProcessorV2 = Vector2.Inf;
        Building closestBuilding = null;
        foreach (Node node in GetTree().GetNodesInGroup("ResourceBuildings"))
        {
            float distance = ((Building)node).GlobalPosition.DistanceTo(GlobalPosition);
            if (distance < closestProcessorV2.DistanceTo(GlobalPosition))
            {
                closestProcessorV2 = ((Building)node).GlobalPosition;
                closestBuilding = (Building)node;
            }
        }
        GD.Print("Finding closest processor...");
        return closestBuilding;
    }
    public Resource FindNearbyResources()
    {
        Vector2 ClosestResource = new Vector2(float.MaxValue, float.MaxValue);
        Resource closestResource = null;
        // Implement logic to find nearby resources
        foreach (Resource resource in GetTree().GetNodesInGroup("Resources"))
        {
            float distance = resource.GlobalPosition.DistanceTo(GlobalPosition);
            if (distance < ClosestResource.DistanceTo(GlobalPosition))
            {
                ClosestResource = resource.GlobalPosition;
                closestResource = resource;
            }
        }
        return closestResource;
    }
    public void AttachResource(Resource resource)
    {
        if (resource.resourceShip != this)
        {
            GD.Print("Resource is already attached to another ship: " + resource.Name);
            return;
        }
        if (GlobalPosition.DistanceTo(resource.GlobalPosition) < PickupRange && !resource.isAttached && !resource.isAttaching)
        {
            // Set the resource's position to match the ship's position
            // This will make the resource lerp toward the ship
            // the Vector2(0, 0) is the offset from the ship's position
            resource.startPosition = GlobalPosition + offset;

            // Mark the resource as attached to this ship
            resource.RemoveFromGroup("Resources");
            resource.AddToGroup("AttachedResources");

            GD.Print("Resource attached: " + resource.Name);

            // Create a tractor beam effect
            if (tractorBeam == null)
            {
                tractorBeam = new Line2D();
                AddChild(tractorBeam);
                tractorBeam.DefaultColor = Colors.Green;
                tractorBeam.Width = 2;
                tractorBeam.GlobalPosition = GlobalPosition;
            }

            resource.isAttaching = true;
            path.RemoveAt(0);
        }
        else if (GlobalPosition.DistanceTo(resource.GlobalPosition) > PickupRange)
        {
            GD.Print("Resource is out of range: " + resource.Name);
        }
        else if (resource.GlobalPosition.DistanceTo(resource.startPosition) < 5f)
        {
            resource.isAttached = true;
            resource.isAttaching = false;
            tractorBeam.QueueFree();
            tractorBeam = null;
        }
    }
}