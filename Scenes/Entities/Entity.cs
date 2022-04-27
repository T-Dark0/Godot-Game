using System;
using System.Threading.Tasks;
using Godot;

public abstract class Entity : Node2D
{
#nullable disable //initialized in _Ready or in Initialize
    public Health Health;
    public Sprite Sprite;
    public Tween Tween;
    public int Id;
#nullable enable

    public Vector2i Coords { get; private set; }

    public async Task<MoveResult> Move(Level level, Direction direction, uint speed)
    {
        var destination = Coords + (Vector2i)direction;
        if (!level.IsPassable(destination)) return MoveResult.Failure;

        level.EntityPositions.Remove(Coords);
        Coords = destination;
        level.EntityPositions.Add(Coords, this);
        var targetPosition = level.Map.GlobalPositionOfTile(Coords);

        if (level.Map.IsVisible(destination))
        {
            var distance = direction.IsLinear() ? 1.0f : (float)Math.Sqrt(2);
            var time = distance / speed;
            Tween.InterpolateProperty(this, "global_position", null, targetPosition, time);
            Tween.Start();
            await ToSignal(Tween, "tween_completed");
        }
        else
        {
            GlobalPosition = targetPosition;
        }
        return MoveResult.Success;
    }

    public override void _Ready()
    {
        Health = GetNode<Health>("Health");
        Sprite = GetNode<Sprite>("Sprite");
        Tween = GetNode<Tween>("Tween");
    }

    public void Initialize(Level level, Vector2i coords, int id)
    {
        Coords = coords;
        GlobalPosition = level.Map.GlobalPositionOfTile(coords);
        Id = id;
    }

    public abstract Task PlayTurn(Level level);

    public enum MoveResult
    {
        Success,
        Failure,
    }
}
