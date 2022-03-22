using System;
using Godot;

public class Root : Node2D
{
    private Player _player;
    private Map _map;

    public override void _Ready()
    {
        _map = GetNode<Map>("Map");
        _player = GetNode<Player>("Player");
        WorldGenerator.Generate(Guid.NewGuid().GetHashCode(), Globals.MapSize, Globals.MapEdgeBoundary, _map);
    }

    public override void _Process(float delta)
    {
        OS.SetWindowTitle($"{_player.Coords}");
    }
}
