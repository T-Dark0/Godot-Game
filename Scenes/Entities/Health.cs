using Godot;

public class Health : Node
{
    [Export]
    public uint Current;
    [Export]
    public uint Max;

    [Signal]
    public delegate void Died(Health self);

    public void TakeDamage(uint amount)
    {
        if (amount >= Current)
        {
            Current = 0;
            EmitSignal(nameof(Died), this);
        }
        else
        {
            Current -= amount;
        }
    }
}