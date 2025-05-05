using Godot;

public partial class Ship : Node2D
{
    public ShipMesh Mesh;

    public override void _Ready()
    {
        Mesh = new PlayerShipMesh();
        Mesh.Scale = 25; // Set the scale of the ship
        AddChild(Mesh);
    }
}