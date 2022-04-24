using Godot;

public class Health : Node
{
    [Export]
    public uint Current;
    [Export]
    public uint Max;

    public DamageResult TakeDamage(uint amount)
    {
        if (amount >= Current)
        {
            Current = 0;
            return DamageResult.Dead;
        }
        else
        {
            Current -= amount;
            return DamageResult.Survived;
        }
    }
}

public enum DamageResult
{
    Survived,
    Dead,
}