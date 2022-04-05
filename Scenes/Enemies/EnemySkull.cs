using Godot;

public class EnemySkull : Sprite, Entity
{
    public override void _Ready()
    {
    }

    public void PlayTurn(Level world)
    {
        GD.Print("updating skull");
    }
}
