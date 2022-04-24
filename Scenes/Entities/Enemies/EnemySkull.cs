using System.Threading.Tasks;
using Godot;

public class EnemySkull : Entity
{
    public override async Task PlayTurn(Level level)
    {
        if (level.Map.IsVisible(Coords))
        {
            await ToSignal(GetTree().CreateTimer(1), "timeout");
        }
    }
}
