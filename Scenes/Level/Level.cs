using System;
using System.Collections.Generic;
using GameMap;
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

            for (int i = 0; i < _entities.Count; i++)
            {
                await _entities[i].PlayTurn(this);
            }
            if (_turnCounter % SPAWN_INTERVAL == 0)
            {
                SpawnRandomEnemy();
            }
        }
    }

    public bool IsPassable(Vector2i coord)
    {
        return Map.IsPassable(coord) && !EntityPositions.ContainsKey(coord);
    }

    private void SpawnPlayer(Player player)
    {
        var tile = Map.GetRandomTileCoord(_rng, Tile.Floor);
        player.Initialize(this, tile, _entities.Count);
        player.Health.Connect(nameof(Health.Died), this, nameof(OnEntityDeath));
        _entities.Add(player);
        EntityPositions.Add(player.Coords, player);
    }

    private void SpawnRandomEnemy()
    {
        var enemy = Enemies.List[_rng.Next(Enemies.List.Length)].Instance<Entity>();
        var tile = Map.GetRandomTileCoord(_rng, Tile.Floor);
        if (EntityPositions.ContainsKey(tile)) return; //don't spawn enemies into other things
        AddChild(enemy);
        enemy.Initialize(this, tile, _entities.Count);
        enemy.Health.Connect(nameof(Health.Died), this, nameof(OnEntityDeath));
        _entities.Add(enemy);
        EntityPositions.Add(enemy.Coords, enemy);
    }

    private void OnEntityDeath(Health health)
    {
        var entity = (Entity)health.Owner;

        var last = _entities.Count - 1;
        var lastEntity = _entities[last];
        lastEntity.Id = entity.Id;
        _entities[entity.Id] = lastEntity;
        _entities.RemoveAt(last);
        EntityPositions.Remove(entity.Coords);

        RemoveChild(entity);
        entity.QueueFree();
    }
}