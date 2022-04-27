using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameMap;
using Godot;

public class Level : Node2D
{
#nullable disable //initialized in _Ready or in Initialize
    public Map Map;
    public Random Rng;
    public Player Player;
#nullable enable
    public Dictionary<Vector2i, Entity> EntityPositions = new Dictionary<Vector2i, Entity>();
    private List<Entity> _entities = new List<Entity>();
    private ulong _turnCounter = 0;
    private bool _isPlayerDead = false;

    private const int SPAWN_INTERVAL = 36;

    public override void _Ready()
    {
        Map = GetNode<Map>("Map");
        Player = GetNode<Player>("Player");
    }

    public void Initialize(Random rng)
    {
        Rng = rng;

        Map.Initialize(rng);
        SpawnPlayer(Player);
        Map.RevealAround(Player.Coords, Player.VISION_RADIUS);
    }

    public async void PlayGame()
    {
        await GameLoop();
        //TODO: Make the game over screen actually exist
        GD.Print("Game over!");
        GD.Print("Thank you for playing!");
    }

    public async Task GameLoop()
    {
        while (true) //TODO: game end condition
        {
            _turnCounter++;
            GD.Print("turn ", _turnCounter);

            for (int i = 0; i < _entities.Count; i++)
            {
                await _entities[i].PlayTurn(this);
                GD.Print("turn played");
                if (_isPlayerDead) return;
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
        var tile = Map.GetRandomTileCoord(Rng, Tile.Floor);
        player.Initialize(this, tile, _entities.Count);
        player.Health.Connect(nameof(Health.Died), this, nameof(OnEntityDeath));
        player.Health.Connect(nameof(Health.Died), this, nameof(OnPlayerDeath));
        _entities.Add(player);
        EntityPositions.Add(player.Coords, player);
    }

    private void SpawnRandomEnemy()
    {
        var enemy = Enemies.List[Rng.Next(Enemies.List.Length)].Instance<Entity>();
        var tile = Map.GetRandomTileCoord(Rng, Tile.Floor);
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

    private void OnPlayerDeath(Health _)
    {
        _isPlayerDead = true;
    }
}