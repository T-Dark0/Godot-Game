using System;
using System.Linq;
using Godot;

public class Root : Node2D
{
#nullable disable //Initialized in _Ready
    private Player _player;
    private Level _level;
#nullable enable
    public override void _Ready()
    {
        _level = GetNode<Level>("Level");

        _level.Initialize(new Random());
        _level.PlayGame();
    }
}
