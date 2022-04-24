using Godot;

public class LightArrowFactory : ProjectileFactory
{
    public static readonly LightArrowFactory Instance = new LightArrowFactory();

    private static readonly PackedScene _scene = GD.Load<PackedScene>("res://Scenes/Projectiles/LightArrow.tscn");

    Projectile ProjectileFactory.Create()
    {
        return _scene.Instance<Projectile>();
    }
}