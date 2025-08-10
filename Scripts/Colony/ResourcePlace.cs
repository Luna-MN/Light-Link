using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
[GlobalClass]
public partial class ResourcePlace : Building
{
    public List<Resource> resources = new List<Resource>();
    public Dictionary<Properties.Type, int> resourceCounts = new Dictionary<Properties.Type, int>() { { Properties.Type.Rock, 0 }, { Properties.Type.Ice, 1 }, { Properties.Type.Iron, 2 }, { Properties.Type.Carbon, 3 } };
    public override void _Ready()
    {
        base._Ready();
        AddToGroup("ResourceBuildings");
        Mesh = new MeshInstance2D();
        Mesh.Mesh = new SphereMesh();
        Mesh.Scale = new Vector2(10f, 10f);
        AddChild(Mesh);
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        ProcessResource();
    }
    public void AddResource(Resource resource)
    {
        if (resource != null && !resources.Contains(resource))
        {
            resources.Add(resource);
            GD.Print("Added resource of type: " + resource.type);
        }
    }
    public void ProcessResource()
    {
        // Logic to process the resources, e.g., convert them to a different form or send them to a processor
        foreach (var resource in resources.ToList())
        {
            // Example processing logic
            GD.Print("Processing resource of type: " + resource.type);
            // Here you can add logic to convert or use the resource
            if (GlobalPosition.DistanceTo(resource.GlobalPosition) > 10f)
            {
                // If the resource is too far, remove it from the list
                resources.Remove(resource);
                resource.QueueFree(); // Free the resource node
                ((Camera)GetViewport().GetCamera2D()).resourceCounts[resource.type]++;
                GD.Print("Removed resource due to distance: " + resource.type);
            }
        }
    }
}

