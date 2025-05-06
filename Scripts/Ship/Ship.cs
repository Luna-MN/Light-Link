using System.Text.RegularExpressions;
using Godot;

public partial class Ship : Node2D
{
    public ShipMesh Mesh;
    public bool shipSelected = false;
    public bool isMine = false;
    public Vector2 targetPosition;
    public Control selectionBrackets;
    public float speed = 100f;
    public Area2D area2D;
    public CollisionShape2D collisionShape;
    public CircleShape2D circleShape;
    public TrailEffect trailEffect;
    public float rotationSpeed = 5.0f;
    public override void _Ready()
    {
        AddToGroup("Ships");
        targetPosition = Position;
        area2D = new Area2D();
        area2D.Position = new Vector2(0, 0);
        AddChild(area2D);
        collisionShape = new CollisionShape2D();
        area2D.AddChild(collisionShape);

        CreateSelectionBrackets();
        selectionBrackets.Scale *= new Vector2(5f, 5f);
        selectionBrackets.Visible = false;

        circleShape = new CircleShape2D();
        circleShape.Radius = 20;
        collisionShape.Shape = circleShape;
        collisionShape.Position = new Vector2(0, 0);

        trailEffect = new TrailEffect();
        trailEffect.TrailLength = 100;
        trailEffect.SetTrailWidth(10);
        trailEffect.DefaultColor = new Color(0.2f, 0.6f, 1.0f).Darkened(0.4f);
        AddChild(trailEffect);
    }
    public override void _Process(double delta)
    {

        MoveShip((float)delta);
        if (shipSelected)
        {
            selectionBrackets.Visible = true;
            selectionBrackets.GlobalPosition = GlobalPosition;
        }
        else
        {
            selectionBrackets.Visible = false;
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
    private void CreateSelectionBrackets()
    {
        selectionBrackets = new Control();
        selectionBrackets.Name = "SelectionBrackets";
        AddChild(selectionBrackets);
        selectionBrackets.TopLevel = true;
        // Create smaller brackets
        CreateBracket("TopLeftBracket", new Vector2(-1, -1), new Vector2(-0.5f, -1), new Vector2(-1, -0.5f));
        CreateBracket("TopRightBracket", new Vector2(1, -1), new Vector2(0.5f, -1), new Vector2(1, -0.5f));
        CreateBracket("BottomLeftBracket", new Vector2(-1, 1), new Vector2(-0.5f, 1), new Vector2(-1, 0.5f));
        CreateBracket("BottomRightBracket", new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(1, 0.5f));
    }
    private void CreateBracket(string name, Vector2 corner, Vector2 horizontalEnd, Vector2 verticalEnd)
    {
        var bracket = new Line2D();
        bracket.Name = name;
        bracket.Width = 0.5f;
        bracket.DefaultColor = Colors.White;
        // Create smaller brackets (reduce from 40 to 10)
        bracket.AddPoint(corner * 5);
        bracket.AddPoint(horizontalEnd * 5);
        bracket.AddPoint(corner * 5);
        bracket.AddPoint(verticalEnd * 5);

        selectionBrackets.AddChild(bracket);
    }
}