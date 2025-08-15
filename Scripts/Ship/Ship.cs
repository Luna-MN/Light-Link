using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Godot;
[GlobalClass]
public partial class Ship : Node2D
{
    public ShipMesh Mesh;
    public bool shipSelected = false;
    [Export]
    public bool isMine = false;

    [Export] public int health;
    public float speed = 100f;
    public Area2D area2D;   
    public CollisionShape2D collisionShape;
    public ConvexPolygonShape2D shipShape;
    public SelectionBracket selectionBrackets = new SelectionBracket();
    public TrailEffect trailEffect;
    public float rotationSpeed = 5.0f;
    public List<Vector2> path = new List<Vector2>();

    // Add these new variables
    private Line2D pathLine;
    private Node2D waypointMarkers;

    // Component properties
    public List<AttachmentPoint> attachmentPoints = new List<AttachmentPoint>();
    public List<Component> attachedComponents = new List<Component>();
    public int powerLeft = 0;

    public override void _Ready()
    {
        AddToGroup("Ships");
        path.Add(Position);
        area2D = new Area2D();
        area2D.Position = new Vector2(0, 0);
        AddChild(area2D);
        collisionShape = new CollisionShape2D();
        area2D.AddChild(collisionShape);

        //CreateSelectionBrackets();
        //selectionBrackets.Scale *= new Vector2(5f, 5f);
        //selectionBrackets.Visible = false;

        shipShape = new ConvexPolygonShape2D();
        collisionShape.Shape = shipShape;
        collisionShape.Position = new Vector2(0, 0);

        trailEffect = new TrailEffect();
        trailEffect.TrailLength = 100;
        trailEffect.SetTrailWidth(10);
        trailEffect.DefaultColor = new Color(0.2f, 0.6f, 1.0f).Darkened(0.4f);
        AddChild(trailEffect);

        // Create path visualization elements
        CreatePathVisuals();
        GetTree().Root.GetChildren().FirstOrDefault()?.GetNode<HitDetector>("HitDetector").RegisterShip(this);
    }

    public override void _Process(double delta)
    {

        MoveShip((float)delta);

        // Update selection brackets
        if (shipSelected)
        {
            selectionBrackets.Visible = true;
            selectionBrackets.GlobalPosition = GlobalPosition;
            // Only show path when selected
            UpdatePathVisuals();
        }
        else
        {
            selectionBrackets.Visible = false;
            pathLine.Visible = false;
            waypointMarkers.Visible = false;
        }
    }

    public void MoveShip(float delta)
    {
        if (path.Count == 0)
        {
            return;
        }
        Vector2 targetPosition = path[0];
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
        else if (path.Count > 1)
        {
            path.RemoveAt(0); // Remove the first point if we are close enough
            if (shipSelected)
            {
                UpdatePathVisuals(); // Update visual after removing a point
            }
        }
    }
    private void CreatePathVisuals()
    {
        // Create the line for the path
        pathLine = new Line2D();
        pathLine.Name = "PathLine";
        pathLine.Width = 1.0f;
        pathLine.DefaultColor = Colors.White;
        AddChild(pathLine);
        // Make path line top-level so it stays in world space
        pathLine.TopLevel = true;

        // Create a container for waypoint markers
        waypointMarkers = new Node2D();
        waypointMarkers.Name = "WaypointMarkers";
        AddChild(waypointMarkers);
        // Make waypoint markers top-level
        waypointMarkers.TopLevel = true;

        // Initially hide both
        pathLine.Visible = false;
        waypointMarkers.Visible = false;
    }

    private void UpdatePathVisuals()
    {
        // Show path elements
        pathLine.Visible = true;
        waypointMarkers.Visible = true;

        // Clear existing points
        pathLine.ClearPoints();

        // Remove old waypoint markers
        foreach (Node child in waypointMarkers.GetChildren())
        {
            child.QueueFree();
        }

        // Start from current position (use global position)
        pathLine.AddPoint(GlobalPosition);

        // Add each path point (use global coordinates)
        for (int i = 0; i < path.Count; i++)
        {
            // Use global path points directly
            Vector2 globalPoint = path[i];
            pathLine.AddPoint(globalPoint);

            // Only create waypoint markers for future waypoints (skip current target)
            if (i > 0 || GlobalPosition.DistanceSquaredTo(path[0]) > 25)
            {
                CreateWaypointMarker(globalPoint, i);
            }
        }
    }

    private void CreateWaypointMarker(Vector2 position, int index)
    {
        // Create waypoint circle
        var marker = new Node2D();
        marker.Name = $"Waypoint{index}";
        waypointMarkers.AddChild(marker);
        marker.Position = position;

        // White outer circle
        var outerCircle = new MeshInstance2D();
        outerCircle.Name = "OuterCircle";
        outerCircle.Mesh = new SphereMesh();
        outerCircle.Scale = new Vector2(5f, 5f);
        outerCircle.Modulate = Colors.White;

        // Black inner circle
        var innerCircle = new MeshInstance2D();
        innerCircle.Name = "InnerCircle";
        innerCircle.Mesh = new SphereMesh();
        innerCircle.Scale = new Vector2(3f, 3f);
        innerCircle.Modulate = Colors.Black;

        marker.AddChild(outerCircle);
        marker.AddChild(innerCircle);

    }

    // This method should be called whenever the path list changes
    public void SetDestination(Vector2 destination)
    {
        path.Clear();
        path.Add(destination);

        if (shipSelected)
        {
            UpdatePathVisuals();
        }
    }
    public Vector2 FindOrbitInterceptPoint(Planet targetPlanet, float maxPredictionTime = 20.0f)
    {
        // Ship parameters
        Vector2 shipPosition = GlobalPosition;

        // Planet parameters
        Vector2 planetPosition = targetPlanet.GlobalPosition;
        Vector2 starPosition = ((Node2D)targetPlanet.GetParent()).GlobalPosition;
        float orbitRadius = targetPlanet.Properties.OrbitRadius;
        float orbitPeriod = targetPlanet.Properties.OrbitPeriod;

        // Calculate orbital velocity (radians per time unit)
        float angularVelocity = 2 * Mathf.Pi / orbitPeriod;

        // Calculate current planet angle in orbit
        Vector2 relativePos = planetPosition - starPosition;
        float currentAngle = Mathf.Atan2(relativePos.Y, relativePos.X);

        // Variables to track best intercept
        float bestTime = 0;
        float bestDistance = float.MaxValue;

        // Sample time points to find intercept
        float timeStep = 0.1f;
        for (float t = 0; t <= maxPredictionTime; t += timeStep)
        {
            // Calculate planet position at time t
            float futureAngle = currentAngle + angularVelocity * t;
            Vector2 futurePlanetPosition = starPosition + new Vector2(
                orbitRadius * Mathf.Cos(futureAngle),
                orbitRadius * Mathf.Sin(futureAngle)
            );

            // How far the ship can travel in time t
            float maxShipTravel = speed * t;

            // Actual distance to future planet position
            float distanceToPlanet = shipPosition.DistanceTo(futurePlanetPosition);

            // Find the closest match between travel time and distance
            float difference = Mathf.Abs(distanceToPlanet - maxShipTravel);
            if (difference < bestDistance)
            {
                bestDistance = difference;
                bestTime = t;
            }
        }

        // Calculate final intercept position
        float interceptAngle = currentAngle + angularVelocity * bestTime;
        Vector2 interceptPosition = starPosition + new Vector2(
            orbitRadius * Mathf.Cos(interceptAngle),
            orbitRadius * Mathf.Sin(interceptAngle)
        );

        return interceptPosition;
    }
    public void AddAttachmentPoint(Vector3 attachmentPoint)
    {
        // Create a new AttachmentPoint at the specified position
        AttachmentPoint newAttachmentPoint = new AttachmentPoint();
        newAttachmentPoint.Position = new Vector2(attachmentPoint.X, attachmentPoint.Y);
        newAttachmentPoint.Name = "AttachmentPoint_" + attachmentPoints.Count;
    }
    public void CreateShipAttachmentPoints()
    {
        foreach (Vector3 pos in Mesh.DefineVertices())
        {
            AddAttachmentPoint(pos);
        }
    }
}