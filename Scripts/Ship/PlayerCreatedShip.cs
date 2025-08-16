using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
[GlobalClass]
public partial class PlayerCreatedShip : PlayerShips
{
    [Export] public ShipSave shipSave;
    [Export] public string shipPath;
    public List<AttachmentPoint> shipNodes = new();
    public List<Module> modules = new();
    public List<Gun> guns = new();
    public List<Shield> shields = new();
    
    [Export]
    public Node2D TargetNode = null;
     public override void _Ready()
    {

        Mesh = new PlayerMadeShipMesh(shipSave);
        shipNodes = ((PlayerMadeShipMesh)Mesh).GetShipNodes();
        health = shipNodes.Count * 10;
        Scale = new Vector2(.25f, .25f);
        AddChild(Mesh);
        base._Ready();
        ShipShape();
        selectionBrackets = new SelectionBracket();
        AddChild(selectionBrackets);
        UpdateSelectionBracket();
        ShowNodes(true);
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (TargetNode != null)
        {
            try
            {
                LookTowardsTarget(TargetNode.GlobalPosition);
            }
            catch (Exception e)
            {
                TargetNode = null;
            }

        }
    }

    public void LookTowardsTarget(Vector2 target)
    {
        foreach (var gun in guns)
        {
            gun.target = TargetNode;
        }
    }
    public void ShowNodes(bool vis)
    {
        foreach (var node in shipNodes)
        {
            node.Visible = vis;
        }
    }

    private void ShipShape()
    {
        var points = new List<Vector2>();
        foreach (var node in shipNodes)
            points.Add(node.Position);

        // Only proceed if there are enough points
        if (points.Count < 3)
        {
            GD.PrintErr("Not enough points to form a polygon.");
            return;
        }

        // Call helper to order points CCW around centroid
        var orderedPoints = OrderPointsCCW(points);
        shipShape.Points = orderedPoints.ToArray();
    }
    private List<Vector2> OrderPointsCCW(List<Vector2> points)
    {
        // Compute centroid
        Vector2 centroid = Vector2.Zero;
        foreach (var pt in points)
            centroid += pt;
        centroid /= points.Count;

        // Sort points by angle from centroid
        return points
            .OrderBy(pt => Math.Atan2(pt.Y - centroid.Y, pt.X - centroid.X))
            .ToList();
    }


    private void UpdateSelectionBracket()
    {
        var bounds = GetShipBounds();
        selectionBrackets.CreateSelectionBrackets(bounds);
    }
    private Rect2 GetShipBounds()
    {
        if (shipNodes.Count == 0)
            return new Rect2();

        Vector2 min = shipNodes[0].Position;
        Vector2 max = shipNodes[0].Position;

        foreach (var node in shipNodes)
        {
            Vector2 pos = node.Position;
            min = new Vector2(Math.Min(min.X, pos.X), Math.Min(min.Y, pos.Y));
            max = new Vector2(Math.Max(max.X, pos.X), Math.Max(max.Y, pos.Y));
        }
        return new Rect2(min, max - min);
    }
}
