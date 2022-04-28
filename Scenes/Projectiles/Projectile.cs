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
        var actualTarget = LineOfSight
            .Towards(from, to)
            .TakeWhile(coord => !level.Map.World[coord].BlocksProjectiles())
            .Take(MaxRange)
            .Last();

        level.AddChild(this);
        await this.Animate(level, from, actualTarget);
        level.RemoveChild(this);

        Entity target;
        if (level.EntityPositions.TryGetValue(to, out target))
        {
            target.Health.TakeDamage(Damage);
        }
        QueueFree();
        return actualTarget;
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

public interface ProjectileFactory
{
    public Projectile Create();
}