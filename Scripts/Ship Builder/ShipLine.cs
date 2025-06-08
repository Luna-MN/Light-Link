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
        var segmentShape = new SegmentShape2D();
        segmentShape.A = StartNode.GlobalPosition;
        segmentShape.B = EndNode.GlobalPosition;
        collisionShape.Shape = segmentShape;
        area.AddChild(collisionShape);
        Line.AddChild(area);
    }
}