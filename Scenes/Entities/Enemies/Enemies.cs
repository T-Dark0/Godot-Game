using Godot;

public class Enemies
{
    public static readonly PackedScene Skull = GD.Load<PackedScene>("res://Scenes/Entities/Enemies/EnemySkull.tscn");

    public static readonly PackedScene[] List = { Skull };
}