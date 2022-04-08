using System;
using Godot;

public class Map : Node2D
{
#nullable disable //initialized in _Ready()
    private WorldMap _worldMap;
    private VisibilityMap _visibilityMap;
#nullable enable

    public override void _Ready()
    {
        _worldMap = GetNode<WorldMap>("WorldMap");
        _visibilityMap = GetNode<VisibilityMap>("VisibilityMap");
    }

    public void Initialize(Random rng)
    {
        WorldGenerator.Generate(rng, _worldMap);
        _visibilityMap.BlackOut(_worldMap);
    }
}
