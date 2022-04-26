using System.Threading.Tasks;
using Godot;

public class Projectile : Node2D
{
    [Export]
    public float Speed;
    [Export]
    public uint Damage;

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
        var globalFrom = level.Map.GlobalCoordsOfTile(from);
        var globalTo = level.Map.GlobalCoordsOfTile(to);

        GlobalPosition = globalFrom;
        LookAt(globalTo);
        var targetDistance = globalFrom.DistanceTo(globalTo) / Globals.TILE_SIZE;
        var time = targetDistance / Speed;
        Tween.InterpolateProperty(this, "global_position", null, globalTo, time);
        Tween.Start();
        await ToSignal(Tween, "tween_completed");
        return to;
    }
}

public interface ProjectileFactory
{
    public Projectile Create();
}