using System.Threading.Tasks;
using Godot;

public class Actions
{
    public static void MoveEntity(Level level, Entity entity, Vector2i destination)
    {
        entity.Coords = destination;
    }

    public static async Task FireProjectile(Level level, Projectile projectile, Vector2 globalFrom, Vector2 globalTo)
    {
        level.AddChild(projectile);
        projectile.Initialize(globalFrom, globalTo);
        await level.ToSignal(projectile, nameof(Projectile.TargetReached));
        level.RemoveChild(projectile);
        projectile.QueueFree();
    }
}