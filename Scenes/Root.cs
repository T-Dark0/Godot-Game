using System;
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
        _player = GetNode<Player>("Player");

        _level.Initialize(new Random(), _player);
    }
}
