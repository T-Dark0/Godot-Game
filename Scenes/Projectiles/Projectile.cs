using Godot;

public abstract class Projectile : Sprite
{
    protected abstract float Speed { get; }

    private float _distanceTravelled = 0;
    private Vector2 _direction;
    private float _targetDistance;

    [Signal]
    public delegate void TargetReached();

    public void Initialize(Vector2 globalPosition, Vector2 globalDestination)
    {
        GlobalPosition = globalPosition;
        LookAt(globalDestination);
        _direction = globalPosition.DirectionTo(globalDestination);
        _targetDistance = globalPosition.DistanceTo(globalDestination);
    }

    public override void _PhysicsProcess(float delta)
    {
        var distance = Speed * delta;
        Position = Position + _direction * distance;
        _distanceTravelled += distance;
        if (_distanceTravelled > _targetDistance)
        {
            EmitSignal(nameof(TargetReached));
        }
    }
}