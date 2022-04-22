using Godot;

public class LightArrow : Projectile
{
    protected override float Speed => Globals.TILE_SIZE * 40.0f;

    private static readonly PackedScene _scene = GD.Load<PackedScene>("res://Scenes/Projectiles/LightArrow.tscn");

    public static LightArrow Instance()
    {
        return _scene.Instance<LightArrow>();
    }
}