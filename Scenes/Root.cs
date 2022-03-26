using System;
using Godot;

public class Root : Node2D
{
#nullable disable //These fields aren't _really_ nullable: they're set in _Ready, which might as well be a constructor.
    private Player _player;
    private Map _map;
#nullable enable

    public override void _Ready()
    {
        _map = GetNode<Map>("Map");
        _player = GetNode<Player>("Player");
        //var seed = 6;
        var seed = Guid.NewGuid().GetHashCode();
        WorldGenerator.Generate(seed, Globals.MapSize, Globals.MapEdgeBoundary, _map);
    }

    public override void _Process(float delta)
    {
        OS.SetWindowTitle($"{_player.Coords}");
    }
}
