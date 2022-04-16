using System;
using System.Collections.Generic;
using Godot;

public class Level : Node2D
{
#nullable disable //initialized in _Ready or in Initialize
    private WorldMap _worldMap;
    private VisibilityMap _visibilityMap;
    private Player _player;
    private Random _rng;
#nullable enable
    private List<Entity> _entities = new List<Entity>();
    private ulong _turnCounter = 0;

    private const int SPAWN_INTERVAL = 36;

    public override void _Ready()
    {
        _worldMap = GetNode<WorldMap>("WorldMap");
        _visibilityMap = GetNode<VisibilityMap>("VisibilityMap");
    }

    public void Initialize(Random rng, Player player)
    {
        _rng = rng;
        _player = player;

        WorldGenerator.Generate(rng, _worldMap);
        SpawnEntity(_player);
        _visibilityMap.Initialize(_worldMap);
    }

    public async void GameLoop()
    {
        RevealAround(_player.Coords, Player.VISION_RADIUS);

        while (true) //TODO: game end condition
        {
            _turnCounter++;
            GD.Print("turn ", _turnCounter);
            foreach (var entity in _entities)
            {
                await entity.PlayTurn(this);
            }
            if (_turnCounter % SPAWN_INTERVAL == 0)
            {
                SpawnEnemy();
            }
        }
    }

    public void RevealAround(Vector2i viewpoint, int radius)
    {
        _visibilityMap.RevealAround(_worldMap, viewpoint, radius);
    }

    public bool IsVisible(Vector2i point)
    {
        return _visibilityMap[point] == VisibilityTile.Empty;
    }

    private void SpawnEnemy()
    {
        var enemyIndex = _rng.Next(Enemies.List.Length);
        SpawnEntity(Enemies.List[enemyIndex].Instance<Entity>());
    }

    private void SpawnEntity(Entity entity)
    {
        var tile = _worldMap.GetRandomTile(_rng, Tile.Floor);
        entity.Initialize(tile);
        _entities.Add(entity);
        AddChild(entity);
    }
}