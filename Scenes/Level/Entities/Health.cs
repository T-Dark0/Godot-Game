using System;
using Godot;

public class Health : Node
{
    [Export] public uint Current { get; private set; }
    [Export] public uint Max { get; private set; }

    [Signal]
    public delegate void Changed(Health self);

    [Signal]
    public delegate void Died(Health self);

    public void Set(uint amount)
    {
        amount = Math.Min(Max, amount);
        Current = amount;
        EmitSignal(nameof(Changed), this);
        if (Current == 0) EmitSignal(nameof(Died), this);
    }

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
        EmitSignal(nameof(Changed), this);
    }
}