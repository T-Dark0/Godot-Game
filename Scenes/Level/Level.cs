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
    private ulong _turnCounter = 0;

    private const int SPAWN_INTERVAL = 36;

    public override void _Ready()
    {
        _map = GetNode<Map>("Map");
    }

    public void Initialize(Random rng, Player player)
    {
        _rng = rng;
        _entities.Add(player);
        _map.Initialize(rng);
    }

    public async void GameLoop()
    {
        while (true) //TODO: game end condition
        {
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
        var enemy = Enemies.List[enemyIndex].Instance<Entity>();
        _entities.Add(enemy);
        AddChild(enemy);
    }
}