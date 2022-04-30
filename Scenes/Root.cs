using System;
using Godot;

public class Root : Node2D
{
#nullable disable //Initialized in _Ready
    private Level _level;
#nullable enable
    public override void _Ready()
    {
        _level = GetNode<Level>("Level");

        _level.Initialize(new Random());
        _level.PlayGame();
    }
}
