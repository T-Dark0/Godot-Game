using System.Linq;
using System.Threading.Tasks;
using Godot;

public class Projectile : Node2D
{
    private Projectiles.Attributes _attributes;
    public float Speed { get => _attributes.Speed; }
    public uint Damage { get => _attributes.Damage; }
    public uint MaxRange { get => _attributes.MaxRange; }

#nullable disable //Initialized in _Ready
    public Sprite Sprite;
    public Tween Tween;
#nullable enable

    public override void _Ready()
    {
        Sprite = GetNode<Sprite>("Sprite");
        Tween = GetNode<Tween>("Tween");
    }

    public void Initialize(Projectiles.Attributes attributes)
    {
        _attributes = attributes;
    }

    public async Task<Vector2i> Fire(Level level, Vector2i from, Vector2i to)
    {
        var actualTo = _attributes.GetTrajectory(level, from, to)
        .TakeWhileInclusive(pair => pair.Tile == TrajectoryTile.Reachable)
        .Last()
        .Coord;

        level.AddChild(this);
        await Animate(level, from, actualTo);
        level.RemoveChild(this);

        Entity target;
        if (level.EntityPositions.TryGetValue(actualTo, out target))
        {
            target.Health.TakeDamage(Damage);
        }
        QueueFree();
        return actualTo;
    }

    private async Task Animate(Level level, Vector2i from, Vector2i to)
    {
        var globalFrom = level.Map.GlobalPositionOfTile(from);
        var globalTo = level.Map.GlobalPositionOfTile(to);

        GlobalPosition = globalFrom;
        LookAt(globalTo);
        var targetDistance = globalFrom.DistanceTo(globalTo) / Globals.TILE_SIZE;
        var time = targetDistance / Speed;
        Tween.InterpolateProperty(this, "global_position", null, globalTo, time);
        Tween.Start();
        await ToSignal(Tween, "tween_completed");
    }
}

public enum TrajectoryTile
{
    Reachable,
    Blocker,
    Unreachable,
}