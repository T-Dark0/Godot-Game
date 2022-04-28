using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameMap;
using Godot;

public class Projectile : Node2D
{
    [Export]
    public float Speed;
    [Export]
    public uint Damage;
    [Export(PropertyHint.Range, "0,99")]
    public int MaxRange;

#nullable disable //Initialized in _Ready
    public Sprite Sprite;
    public Tween Tween;
#nullable enable

    public override void _Ready()
    {
        Sprite = GetNode<Sprite>("Sprite");
        Tween = GetNode<Tween>("Tween");
    }

    public async Task<Vector2i> Fire(Level level, Vector2i from, Vector2i to)
    {
        var actualTo = GetTrajectory(level, from, to)
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

    public IEnumerable<(Vector2i Coord, TrajectoryTile Tile)> GetTrajectory(Level level, Vector2i from, Vector2i to)
    {
        return GetInfiniteTrajectory(level, from, to).Take(MaxRange);
    }

    private IEnumerable<(Vector2i Coord, TrajectoryTile Tile)> GetInfiniteTrajectory(Level level, Vector2i from, Vector2i to)
    {
        var tile = TrajectoryTile.Reachable;
        var los = LineOfSight.Towards(from, to).GetEnumerator();
        los.MoveNext();
        yield return (los.Current, tile);
        while (los.MoveNext())
        {
            var coord = los.Current;
            if (tile == TrajectoryTile.Reachable
                && level.Map.World[coord].BlocksProjectiles() || level.EntityPositions.ContainsKey(coord)
            )
            {
                tile = TrajectoryTile.Blocker;
            }
            else if (tile == TrajectoryTile.Blocker)
            {
                tile = TrajectoryTile.Unreachable;
            }
            yield return (coord, tile);
        }
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