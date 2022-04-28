using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GameMap;

public class WorldMap : TileMap
{
    public Tile this[Vector2i coords]
    {
        get { return (Tile)GetCell(coords.x, coords.y); }
        set { SetCell(coords.x, coords.y, (int)value); }
    }

    public Vector2i GetRandomTile(Random rng, Tile kind)
    {
        var cells = GetUsedCellsById((int)kind);
        var rand = rng.Next(cells.Count);
        return (Vector2i)(Vector2)cells[rand];
    }

    public IEnumerable<Vector2i> TileCoords()
    {
        foreach (Vector2 coord in GetUsedCells())
        {
            yield return (Vector2i)coord;
        }
    }
}

public readonly struct WorldMapView
{
    private readonly WorldMap _map;

    public WorldMapView(WorldMap map)
    {
        _map = map;
    }

    public Tile this[Vector2i coords]
    {
        get => _map[coords];
    }

    public IEnumerable<Vector2i> TileCoords() => _map.TileCoords();
}

public enum Tile
{
    Empty = -1,
    Wall = 0,
    Floor = 1,
}

public static class TileMethods
{
    public static bool BlocksLight(this Tile tile)
    {
        return tile == Tile.Wall || tile == Tile.Empty;
    }

    public static bool BlocksProjectiles(this Tile tile)
    {
        return tile == Tile.Wall || tile == Tile.Empty;
    }
}