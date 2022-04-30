using Godot;

public class Scenes : Node2D
{
#nullable disable //Initialized in _Ready
    public static Scenes Instance { get; private set; }

    [Export] public PackedScene MainMenu { get; private set; }
    [Export] public PackedScene Level { get; private set; }
    [Export] public PackedScene DeathScreen { get; private set; }
    [Export] public PackedScene EnemySkull { get; private set; }
    [Export] public PackedScene LightArrow { get; private set; }
    [Export] public PackedScene CursedBolt { get; private set; }
#nullable enable

    public override void _Ready()
    {
        Instance = this;
    }

    public static MainMenu InstanceMainMenu() => Instance.MainMenu.Instance<MainMenu>();
    public static Level InstanceLevel() => Instance.Level.Instance<Level>();
    public static DeathScreen InstanceDeathScreen() => Instance.DeathScreen.Instance<DeathScreen>();
    public static EnemySkull InstanceEnemySkull() => Instance.EnemySkull.Instance<EnemySkull>();
    public static Projectile InstanceLightArrow() => Instance.LightArrow.Instance<Projectile>();
    public static Projectile InstanceCursedBolt() => Instance.CursedBolt.Instance<Projectile>();
}