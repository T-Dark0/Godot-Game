using System;
using Godot;

public class Map : Node2D
{
#nullable disable //initialized in _Ready()
    private WorldMap _world;
    private VisibilityMap _visibility;
#nullable enable

    public override void _Ready()
    {
        _world = GetNode<WorldMap>("World");
        _visibility = GetNode<VisibilityMap>("Visibility");
    }

    public void Initialize(Random rng)
    {
        WorldGenerator.Generate(rng, _world);
        _visibility.Initialize(_world);
    }

    public Vector2i GetRandomTileCoord(Random rng, Tile tile)
    {
        return _world.GetRandomTile(rng, tile);
    }

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

    public Vector2i TileAtGlobalCoords(Vector2 globalCoords)
    {
        return (Vector2i)_world.WorldToMap(_world.ToLocal(globalCoords));
    }

    public Vector2 GlobalCoordsOfTile(Vector2i tile)
    {
        return _world.ToGlobal(_world.MapToWorld((Vector2)tile)) + new Vector2(Globals.TILE_SIZE / 2, Globals.TILE_SIZE / 2);
    }
}
