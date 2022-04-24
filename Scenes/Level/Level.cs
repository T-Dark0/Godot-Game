using System;
using System.Collections.Generic;
using Godot;

public class Level : Node2D
{
#nullable disable //initialized in _Ready or in Initialize
    public Map Map;
    private Player _player;
    private Random _rng;
#nullable enable
    private List<Entity> _entities = new List<Entity>();
    public Dictionary<Vector2i, Entity> EntityPositions = new Dictionary<Vector2i, Entity>();
    private ulong _turnCounter = 0;

    private const int SPAWN_INTERVAL = 36;

    public override void _Ready()
    {
        Map = GetNode<Map>("Map");
    }

    public void Initialize(Random rng, Player player)
    {
        _rng = rng;
        _player = player;

        Map.Initialize(rng);
        SpawnPlayer(_player);
        Map.RevealAround(_player.Coords, Player.VISION_RADIUS);
    }

    public async void GameLoop()
    {
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
                SpawnRandomEnemy();
            }
        }
    }

    private void SpawnPlayer(Player player)
    {
        var tile = Map.GetRandomTileCoord(_rng, Tile.Floor);
        player.Initialize(tile);
        _entities.Add(player);
        EntityPositions.Add(player.Coords, player);
    }

    private void SpawnRandomEnemy()
    {
        var enemy = Enemies.List[_rng.Next(Enemies.List.Length)].Instance<Entity>();
        var tile = Map.GetRandomTileCoord(_rng, Tile.Floor);
        enemy.Initialize(tile);
        _entities.Add(enemy);
        AddChild(enemy);
        EntityPositions.Add(enemy.Coords, enemy);
    }
}