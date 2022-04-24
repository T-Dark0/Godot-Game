using System.Threading.Tasks;
using Godot;

public class EnemySkull : Entity
{
    public override async Task PlayTurn(Level level)
    {
        GD.Print("skull turn start");
        if (level.Map.IsVisible(Coords))
        {
            await ToSignal(GetTree().CreateTimer(1), "timeout");
        }
        GD.Print("skull turn end");
    }
}
