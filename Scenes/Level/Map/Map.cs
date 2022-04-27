using System;
using Godot;

namespace GameMap;

public class Map : Node2D
{
#nullable disable //initialized in _Ready()
    private WorldMap _world;
    private VisibilityMap _visibility;
#nullable enable

    public WorldMapView World { get => new WorldMapView(_world); }

    public override void _Ready()
    {
        _world = GetNode<WorldMap>("World");
        _visibility = GetNode<VisibilityMap>("Visibility");
    }

    public void Initialize(Random rng)
    {
        WorldGenerator.Generator.Generate(rng, _world);
        _visibility.Initialize(_world);
    }

    public Vector2i GetRandomTileCoord(Random rng, Tile tile)
    {
        return _world.GetRandomTile(rng, tile);
    }

    public Tile GetWorldTile(Vector2i coords) => _world[coords];
    public VisibilityTile GetVisibilityTile(Vector2i coords) => _visibility[coords];

    public void RevealAround(Vector2i viewpoint, int radius)
    {
        _visibility.RevealAround(_world, viewpoint, radius);
    }

    public bool IsVisible(Vector2i point)
    {
        return _visibility[point] == VisibilityTile.Empty;
    }

    public bool IsPassable(Vector2i point)
    {
        return _world[point] == Tile.Floor;
    }

    public Vector2i TileAtGlobalPosition(Vector2 globalCoords)
    {
        return (Vector2i)_world.WorldToMap(_world.ToLocal(globalCoords));
    }

    public Vector2 GlobalPositionOfTile(Vector2i tile)
    {
        return _world.ToGlobal(_world.MapToWorld((Vector2)tile)) + new Vector2(Globals.TILE_SIZE / 2, Globals.TILE_SIZE / 2);
    }
}
