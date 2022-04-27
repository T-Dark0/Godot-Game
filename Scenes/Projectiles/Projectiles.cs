using Godot;

public class Projectiles
{
    public static readonly PackedScene LightArrowScene = GD.Load<PackedScene>("res://Scenes/Projectiles/LightArrow.tscn");
    public static readonly PackedScene CursedBoltScene = GD.Load<PackedScene>("res://Scenes/Projectiles/CursedBolt.tscn");

    public static Projectile LightArrow() => LightArrowScene.Instance<Projectile>();
    public static Projectile CursedBolt() => CursedBoltScene.Instance<Projectile>();
}