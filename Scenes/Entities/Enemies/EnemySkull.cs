using System.Linq;
using System.Threading.Tasks;

public class EnemySkull : Entity
{
    private const uint MOVE_SPEED = 5;

    private bool _sawPlayerLastTurn = false;

    public override async Task PlayTurn(Level level)
    {
        if (level.Map.IsVisible(Coords))
        {
            if (Coords.DistanceTo(level.Player.Coords) > 12)
            {
                var step = LineOfSight.Between(Coords, level.Player.Coords).ElementAt(1);
                var offset = step - Coords;
                await MoveByOffset(level, offset, MOVE_SPEED);
            }
            else
            {
                await Projectiles.CursedBolt().Fire(level, Coords, level.Player.Coords);
            }
        }
        else
        {
            var dir = Vector2i.NeighborOffsets[level.Rng.Next(8)];
            await MoveByOffset(level, dir, MOVE_SPEED);
        }
    }
}
