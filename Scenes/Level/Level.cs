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
    private const int VISION_RADIUS = 15;

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
        while (true) //TODO: game end condition
        {
            _visibilityMap.RevealAround(_worldMap, _player.Coords, VISION_RADIUS);
            _turnCounter++;
            GD.Print("turn ", _turnCounter);
            foreach (var entity in _entities)
            {
                await entity.PlayTurn();
            }
            if (_turnCounter % SPAWN_INTERVAL == 0)
            {
                SpawnEnemy();
            }
        }
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