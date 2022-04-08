using System.Threading.Tasks;
using Godot;

public class EnemySkull : Entity
{
    public override async Task PlayTurn()
    {
        GD.Print("skull turn start");
        await ToSignal(GetTree().CreateTimer(2), "timeout");
        GD.Print("skull turn end");
    }
}
