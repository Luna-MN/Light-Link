using System.Linq;
using Godot;
[GlobalClass]
public partial class ShipLine : Line2D
{
    private readonly ShipBuilder shipBuilder;
    public ShipNode StartNode { get; set; }
    public ShipNode EndNode { get; set; }
    
    private Area2D area;
    private CollisionShape2D collisionShape;

    public ShipLine(ShipNode start, ShipBuilder shipBuilder)
    {
        StartNode = start;
        StartNode.Modulate = new Color(0, 1, 0); // Change color to green to indicate selection
        EndNode = null; // Initially, the end node is not set
        this.shipBuilder = shipBuilder;
    }
    public ShipLine()
    {
    }

    public override void _Ready()
    {
        if (StartNode == null)
        {
            StartNode = GetParent().GetChildren().FirstOrDefault(x => x.Name == "ShipStartNode") as ShipStartNode;
            SetEndNode(GetParent().GetChildren().FirstOrDefault(x => x.Name == "ShipStartNode2") as ShipStartNode);
            UpdateCollisionShape();        
        }
    }
    public void SetEndNode(ShipNode end)
    {
        if (EndNode != null)
        {
            QueueFree();
        }

        EndNode = end;
        Points = new[] { StartNode.GlobalPosition, EndNode.GlobalPosition };
        Width = 2;
        DefaultColor = Colors.White;
        StartNode.Modulate = StartNode.NodeColor; // Reset color to original after setting end node
        EndNode.ConnectedNodes.Add(StartNode);
        StartNode.ConnectedNodes.Add(EndNode);

        area = new Area2D();
        collisionShape = new CollisionShape2D();

        // Calculate capsule parameters
        Vector2 startPos = StartNode.GlobalPosition;
        Vector2 endPos = EndNode.GlobalPosition;
        Vector2 direction = (endPos - startPos).Normalized();
        float length = startPos.DistanceTo(endPos);

        // Create a capsule shape
        var capsuleShape = new CapsuleShape2D();
        capsuleShape.Radius = 15; // 10 pixels wide total (5 radius on each side)
        capsuleShape.Height = length;

        // Position and rotate the collision shape
        collisionShape.Shape = capsuleShape;
        collisionShape.Position = (startPos + endPos) / 2 - GlobalPosition; // Center between points
        collisionShape.Rotation = direction.Angle() + Mathf.Pi / 2; // Rotate to align with the line (capsules are vertical by default)

        area.AddChild(collisionShape);
        AddChild(area);
    }

    public void UpdateCollisionShape()
    {
        if (collisionShape != null && StartNode != null && EndNode != null)
        {
            Vector2 startPos = StartNode.GlobalPosition;
            Vector2 endPos = EndNode.GlobalPosition;
            Vector2 direction = (endPos - startPos).Normalized();
            float length = startPos.DistanceTo(endPos);

            if (collisionShape.Shape is RectangleShape2D rectShape)
            {
                rectShape.Size = new Vector2(length, 10);
                collisionShape.Position = (startPos + endPos) / 2 - GlobalPosition;
                collisionShape.Rotation = direction.Angle();
            }
            else if (collisionShape.Shape is CapsuleShape2D capsuleShape)
            {
                capsuleShape.Height = length;
                collisionShape.Position = (startPos + endPos) / 2 - GlobalPosition;
                collisionShape.Rotation = direction.Angle() + Mathf.Pi / 2;
            }
        }
    }

    public void LineFollowMouse()
    {
        Width = 2;
        shipBuilder.GetNode("Lines").AddChild(this);
        DefaultColor = Colors.White;
        Points = new Vector2[] { StartNode.GlobalPosition, shipBuilder.GetGlobalMousePosition() };
    }

    public void LineToMose()
    {
        Points = new Vector2[] { StartNode.GlobalPosition, shipBuilder.GetGlobalMousePosition() };
    }
}