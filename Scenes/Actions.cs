using System.Threading.Tasks;

public class Actions
{
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