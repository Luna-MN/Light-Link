using Godot;

public class ShipLine
{
    public ShipNode StartNode { get; set; }
    public ShipNode EndNode { get; set; }
    public ShipBuilder ShipBuilder { get; set; }

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
        Line2D line = new Line2D();
        line.Points = new Vector2[] { StartNode.GlobalPosition, EndNode.GlobalPosition };
        line.Width = 2;
        line.DefaultColor = Colors.White;
        ShipBuilder.AddChild(line);
        StartNode.Modulate = new Color(1, 1, 1);
        EndNode.connectedNodes.Add(StartNode);
        StartNode.connectedNodes.Add(EndNode);
    }
}