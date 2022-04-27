using System.Threading.Tasks;
using Godot;

public class EnemySkull : Entity
{
    private const uint MOVE_SPEED = 5;

    public override async Task PlayTurn(Level level)
    {
        var dir = (Direction)level.Rng.Next(8);
        await Move(level, dir, MOVE_SPEED);
    }
}
