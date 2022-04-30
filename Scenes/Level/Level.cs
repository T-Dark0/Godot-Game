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
    private Label _turnLabel;
#nullable enable
    public Dictionary<Vector2i, Entity> EntityPositions = new Dictionary<Vector2i, Entity>();
    public List<Enemy> Enemies = new List<Enemy>();
    private bool _isPlayerDead = false;

    private const int SPAWN_INTERVAL = 36;

    public override void _Ready()
    {
        Map = GetNode<Map>("Map");
        Player = GetNode<Player>("Player");
        _turnLabel = GetNode<Label>("CanvasLayer/TurnLabel");
    }

    public void Initialize(Random rng)
    {
        Rng = rng;

        Map.Initialize(rng);
        SpawnPlayer(Player);
        Map.RevealAround(Player.Coords, Player.VisionRadius);
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
        var turnCounter = 0;
        while (true)
        {
            turnCounter++;
            _turnLabel.Text = $"Turn: {turnCounter}";

            await Player.PlayTurn(this);
            for (int i = 0; i < Enemies.Count; i++)
            {
                await Enemies[i].PlayTurn(this);
                if (_isPlayerDead) return;
            }
            if (turnCounter % SPAWN_INTERVAL == 0)
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

        player.Initialize(this, tile);
        player.Health.Connect(nameof(Health.Died), this, nameof(OnPlayerDeath));
        player.Connect(nameof(Entity.Moved), this, nameof(OnPlayerMove));

        EntityPositions.Add(player.Coords, player);
    }

    private void SpawnRandomEnemy()
    {
        var enemy = global::Enemies.List[Rng.Next(global::Enemies.List.Length)].Instance<Enemy>();
        var tile = Map.GetRandomTileCoord(Rng, Tile.Floor);
        if (EntityPositions.ContainsKey(tile)) return; //don't spawn enemies into other things
        AddChild(enemy);

        enemy.Initialize(this, tile, Enemies.Count);
        enemy.Health.Connect(nameof(Health.Died), this, nameof(OnEnemyDeath));
        enemy.Connect(nameof(Entity.Moved), this, nameof(OnEnemyMove));

        Enemies.Add(enemy);
        EntityPositions.Add(enemy.Coords, enemy);
    }

    private void OnEnemyDeath(Health health)
    {
        var enemy = (Enemy)health.Owner;

        var last = Enemies.Count - 1;
        var lastEntity = Enemies[last];
        lastEntity.Id = enemy.Id;
        Enemies[enemy.Id] = lastEntity;
        Enemies.RemoveAt(last);
        EntityPositions.Remove(enemy.Coords);

        RemoveChild(enemy);
        enemy.QueueFree();
    }

    private void OnPlayerDeath(Health _)
    {
        _isPlayerDead = true;
    }

    private void OnEnemyMove(Enemy enemy)
    {
        enemy.Visible = Map.IsVisible(enemy.Coords);
    }

    private void OnPlayerMove(Player player)
    {
        foreach (var enemy in Enemies)
        {
            enemy.Visible = Map.IsVisible(enemy.Coords);
        }
    }
}