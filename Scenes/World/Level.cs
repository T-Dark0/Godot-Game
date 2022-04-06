using System;
using System.Collections.Generic;
using Godot;

public class Level : Node2D
{
#nullable disable //initialized in _Ready or in Initialize
    private Map _map;
    private Random _rng;
    private Entity _current_entity;
#nullable enable
    private Queue<Entity> _entities = new Queue<Entity>();
    private ulong _turn_counter = 0;

    private const int SPAWN_INTERVAL = 36;

    [Signal]
    public delegate void PassTurn();

    public override void _Ready()
    {
        _map = GetNode<Map>("Map");
    }

    public void Initialize(Random rng, Player player)
    {
        _rng = rng;

        WorldGenerator.Generate(rng, Globals.MAP_SIZE, _map);
        player.Coords = _map.GetRandomTile(rng, Tile.Floor);


        _current_entity = player;
    }

    public void NextTurn()
    {
        _entities.Enqueue(_current_entity);
        _current_entity = _entities.Dequeue();
        _current_entity.OnTurnStart();
    }

    private void SpawnEnemy()
    {
        var enemyIndex = _rng.Next(Enemies.List.Length);
        var enemy = Enemies.List[enemyIndex].Instance<Entity>();
        enemy.Connect(nameof(PassTurn), this, nameof(NextTurn));
        _entities.Enqueue(enemy);
        AddChild(enemy);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        _current_entity.Input(@event);
    }
}