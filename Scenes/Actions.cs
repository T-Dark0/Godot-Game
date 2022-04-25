using System;
using System.Threading.Tasks;

public class Actions
{
    public static ActionResult MoveEntity(Level level, Entity entity, Vector2i destination)
    {
        level.EntityPositions.Remove(entity.Coords);
        try //if two entities move onto each other, not crashing would probably be a good idea, so we simply declare the action failed
        {
            level.EntityPositions.Add(destination, entity);
            entity.Coords = destination;
        }
        catch (ArgumentException)
        {
            return ActionResult.Failure;
        }
        return ActionResult.Success;
    }

    public static async Task<ActionResult> FireProjectile(Level level, ProjectileFactory factory, Vector2i from, Vector2i to)
    {
        var projectile = factory.Create();

        level.AddChild(projectile);
        var hitTile = await projectile.Fire(level, from, to);
        level.RemoveChild(projectile);

        Entity target;
        if (level.EntityPositions.TryGetValue(hitTile, out target))
        {
            target.Health.TakeDamage(projectile.Damage);
        }

        projectile.QueueFree();
        return ActionResult.Success;
    }
}

public enum ActionResult
{
    Success,
    Failure,
}