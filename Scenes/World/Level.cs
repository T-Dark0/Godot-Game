using System;
using System.Collections.Generic;
using Godot;

public class Level : Node2D
{
#nullable disable //initialized in _Ready or in Initialize
    private Map _map;
    private Random _rng;
#nullable enable
    private List<Entity> _entities = new List<Entity>();
    private ulong _turn_counter = 0;

    private const int SPAWN_INTERVAL = 36;


    public override void _Ready()
    {
        _map = GetNode<Map>("Map");
    }

    public void Initialize(Random rng, Player player)
    {
        _rng = rng;
        _entities.Add(player);

        WorldGenerator.Generate(rng, Globals.MAP_SIZE, _map);
        player.Coords = _map.GetRandomTile(rng, Tile.Floor);
    }

    public async void GameLoop()
    {
        while (true) //TODO: game end condition
        {
            _turn_counter++;
            GD.Print("turn ", _turn_counter);
            foreach (var entity in _entities)
            {
                await entity.PlayTurn();
            }
            if (_turn_counter % SPAWN_INTERVAL == 0)
            {
                SpawnEnemy();
            }
        }
    }

    private void SpawnEnemy()
    {
        var enemyIndex = _rng.Next(Enemies.List.Length);
        var enemy = Enemies.List[enemyIndex].Instance<Entity>();
        _entities.Add(enemy);
        AddChild(enemy);
    }
}