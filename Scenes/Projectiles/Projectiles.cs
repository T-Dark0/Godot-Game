using System.Collections.Generic;
using System.Linq;
using GameMap;
using Godot;

public class Projectiles
{
    public readonly static SceneAndAttributes LightArrow = new SceneAndAttributes(
        scene: GD.Load<PackedScene>("res://Scenes/Projectiles/LightArrow.tscn"),
        attributes: new Attributes(
            speed: 40,
            damage: 20,
            maxRange: 50
        )
    );
    public readonly static SceneAndAttributes CursedBolt = new SceneAndAttributes(
        scene: GD.Load<PackedScene>("res://Scenes/Projectiles/CursedBolt.tscn"),
        attributes: new Attributes(
            speed: 20,
            damage: 10,
            maxRange: 40
        )
    );

    public static Projectile SpawnLightArrow() => Spawn(LightArrow);
    public static Projectile SpawnCursedBolt() => Spawn(CursedBolt);

    public static Projectile Spawn(SceneAndAttributes proj)
    {
        var projectile = proj.Scene.Instance<Projectile>();
        projectile.Initialize(proj.Attributes);
        return projectile;
    }


    public readonly struct SceneAndAttributes
    {
        public readonly PackedScene Scene;
        public readonly Attributes Attributes;

        public SceneAndAttributes(PackedScene scene, Attributes attributes)
        {
            Scene = scene;
            Attributes = attributes;
        }
    }

    public readonly struct Attributes
    {
        public readonly float Speed;
        public readonly uint Damage;
        public readonly uint MaxRange;

        public Attributes(float speed, uint damage, uint maxRange)
        {
            Speed = speed;
            Damage = damage;
            MaxRange = maxRange;
        }

        public IEnumerable<(Vector2i Coord, TrajectoryTile Tile)> GetTrajectory(Level level, Vector2i from, Vector2i to)
        {
            return GetInfiniteTrajectory(level, from, to).Take((int)MaxRange);
        }

        private IEnumerable<(Vector2i Coord, TrajectoryTile Tile)> GetInfiniteTrajectory(Level level, Vector2i from, Vector2i to)
        {
            var tile = TrajectoryTile.Reachable;
            var los = LineOfSight.Towards(from, to).GetEnumerator();
            los.MoveNext();
            yield return (los.Current, tile);
            while (los.MoveNext())
            {
                var coord = los.Current;
                if (tile == TrajectoryTile.Reachable
                    && level.Map.World[coord].BlocksProjectiles() || level.EntityPositions.ContainsKey(coord)
                )
                {
                    tile = TrajectoryTile.Blocker;
                }
                else if (tile == TrajectoryTile.Blocker)
                {
                    tile = TrajectoryTile.Unreachable;
                }
                yield return (coord, tile);
            }
        }
    }
}