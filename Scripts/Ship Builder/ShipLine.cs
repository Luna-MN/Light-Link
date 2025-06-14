using Godot;

public class ShipLine
{
    public ShipNode StartNode { get; set; }
    public ShipNode EndNode { get; set; }
    public ShipBuilder ShipBuilder { get; set; }
    public Line2D Line { get; set; }
    public Area2D area;
    public CollisionShape2D collisionShape;

    public ShipLine(ShipNode start, ShipBuilder shipBuilder)
    {
        StartNode = start;
        StartNode.Modulate = new Color(0, 1, 0); // Change color to green to indicate selection
        EndNode = null; // Initially, the end node is not set
        ShipBuilder = shipBuilder;


    }
    public void SetEndNode(ShipNode end)
    {
        EndNode = end;
        Line = new Line2D();
        Line.Points = new Vector2[] { StartNode.GlobalPosition, EndNode.GlobalPosition };
        Line.Width = 2;
        Line.DefaultColor = Colors.White;
        ShipBuilder.AddChild(Line);
        StartNode.Modulate = StartNode.nodeColor; // Reset color to original after setting end node
        EndNode.connectedNodes.Add(StartNode);
        StartNode.connectedNodes.Add(EndNode);

        area = new Area2D();
        collisionShape = new CollisionShape2D();

        // Calculate capsule parameters
        Vector2 startPos = StartNode.GlobalPosition;
        Vector2 endPos = EndNode.GlobalPosition;
        Vector2 direction = (endPos - startPos).Normalized();
        float length = startPos.DistanceTo(endPos);

        // Create a capsule shape
        var capsuleShape = new CapsuleShape2D();
        capsuleShape.Radius = 5; // 10 pixels wide total (5 radius on each side)
        capsuleShape.Height = length;

        // Position and rotate the collision shape
        collisionShape.Shape = capsuleShape;
        collisionShape.Position = (startPos + endPos) / 2 - Line.GlobalPosition; // Center between points
        collisionShape.Rotation = direction.Angle() + Mathf.Pi / 2; // Rotate to align with the line (capsules are vertical by default)

        area.AddChild(collisionShape);
        Line.AddChild(area);
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
                collisionShape.Position = (startPos + endPos) / 2 - Line.GlobalPosition;
                collisionShape.Rotation = direction.Angle();
            }
            else if (collisionShape.Shape is CapsuleShape2D capsuleShape)
            {
                capsuleShape.Height = length;
                collisionShape.Position = (startPos + endPos) / 2 - Line.GlobalPosition;
                collisionShape.Rotation = direction.Angle() + Mathf.Pi / 2;
            }
        }
    }
}