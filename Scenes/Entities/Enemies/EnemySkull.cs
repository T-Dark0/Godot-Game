using Godot;

public class EnemySkull : Entity
{
    public override void OnTurnStart()
    {
        GD.Print($"skull@{Coords}");
    }
}
