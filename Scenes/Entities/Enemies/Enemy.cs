using System.Threading.Tasks;

public abstract class Enemy : Entity
{
    public int Id;

    public void Initialize(Level level, Vector2i coords, int id)
    {
        base.Initialize(level, coords);
        Id = id;
    }

    public new async Task<MoveResult> MoveByOffset(Level level, Vector2i offset, uint speed, bool animate = true)
    {
        var result = await base.MoveByOffset(level, offset, speed, animate);
        if (result == MoveResult.Success) EmitSignal(nameof(Moved), this);
        return result;
    }
}