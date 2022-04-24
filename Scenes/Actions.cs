using System.Threading.Tasks;

public class Actions
{
    public static void MoveEntity(Level level, Entity entity, Vector2i destination)
    {
        level.EntityPositions.Remove(entity.Coords);
        entity.Coords = destination;
        level.EntityPositions.Add(entity.Coords, entity); //TODO: this throws if two entities move onto each other
    }

    public static async Task FireProjectile(Level level, ProjectileFactory factory, Vector2i from, Vector2i to)
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
    }
}