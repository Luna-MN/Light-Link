using Godot;

public partial class Ship : Node2D
{
    public ShipMesh Mesh;
    public bool shipSelected = false;
    public bool isMine = true;
    public Vector2 targetPosition;
    public float speed = 100f;
    public Area2D area2D;
    public CollisionShape2D collisionShape;
    public CircleShape2D circleShape;
    public float rotationSpeed = 5.0f;
    public override void _Ready()
    {
        Mesh = new PlayerShipMesh();
        Mesh.Scale = 25; // Set the scale of the ship
        AddChild(Mesh);

        area2D = new Area2D();
        area2D.Position = new Vector2(0, 0);
        AddChild(area2D);
        collisionShape = new CollisionShape2D();
        area2D.AddChild(collisionShape);

        circleShape = new CircleShape2D();
        circleShape.Radius = 20;
        collisionShape.Shape = circleShape;
        collisionShape.Position = new Vector2(0, 0);

        TrailEffect trailEffect = new TrailEffect();
        trailEffect.TrailLength = 100;
        trailEffect.SetTrailWidth(10);
        trailEffect.DefaultColor = new Color(0.2f, 0.6f, 1.0f).Darkened(0.2f);
        AddChild(trailEffect);
    }
    public override void _Process(double delta)
    {

        MoveShip((float)delta);

    }
    public void SetShipTarget()
    {
        if (shipSelected && isMine)
        {
            // Get the mouse position in global coordinates
            targetPosition = GetGlobalMousePosition();
        }
    }


    public void MoveShip(float delta)
    {
        // Get the direction vector to the target
        Vector2 direction = targetPosition - Position;
        float distance = direction.Length();

        if (distance > 0.1f) // Only rotate and move if we have some meaningful distance
        {
            // Calculate the angle of the direction vector
            // Add 90 degrees (or PI/2 radians) to make the ship face the movement direction
            float targetAngle = Mathf.Atan2(direction.Y, direction.X) + Mathf.Pi / 2;

            // Smoothly rotate towards the target angle
            Rotation = Mathf.LerpAngle(Rotation, targetAngle, rotationSpeed * delta);

            // If we're very close to the target, just snap to it
            if (distance < speed * delta)
            {
                Position = targetPosition;
            }
            // Otherwise, move with consistent speed
            else
            {
                direction = direction.Normalized();
                Position += direction * speed * delta;
            }
        }
    }
}