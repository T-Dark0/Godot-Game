using System;
using Godot;

public class Map : TileMap
{
    public Graph<Room, Path> Graph;
    public int width { get; private set; }
    public int height { get; private set; }

    private Map()
    {
        Graph = new Graph<Room, Path>();
    }

    public Tile this[int x, int y]
    {
        get
        {
            return (Tile)GetCell(x, y);
        }
        set
        {
            SetCell(x, y, (int)value);
        }
    }
    public Tile this[Vector2i coords]
    {
        get { return this[coords.x, coords.y]; }
        set { this[coords.x, coords.y] = value; }
    }

    public Vector2i GetRandomTile(Random rng, Tile kind)
    {
        var cells = GetUsedCellsById((int)kind);
        var rand = rng.Next(cells.Count);
        return (Vector2i)(Vector2)cells[rand];
    }
}