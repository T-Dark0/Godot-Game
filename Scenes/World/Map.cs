using System.Collections.Generic;
using Godot;

public class Map : TileMap
{
    public List<Room> Rooms;
    public List<Path> Paths;
    public int width { get; private set; }
    public int height { get; private set; }

    private Map()
    {
        Rooms = new List<Room>();
        Paths = new List<Path>();
    }

    public static Map Filled(int width, int height, Tile tile)
    {
        var self = new Map();
        self.width = width;
        self.height = height;
        return self;
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }
    public bool IsInBounds(Vector2i coords)
    {
        return IsInBounds(coords.x, coords.y);
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

    public override void _Draw()
    {
        foreach (var path in Paths)
        {
            for (int i = 0; i < path.Points.Count - 1; i++)
            {
                var from = (Vector2)path.Points[i] * Globals.TileSize;
                var to = (Vector2)path.Points[i + 1] * Globals.TileSize;
                DrawLine(from, to, new Color("FF00FF00"));
            }
        }

        foreach (var room in Rooms)
        {
            var origin = room.BoundingBox.Origin * 16;
            var size = room.BoundingBox.Size * 16;
            DrawRect(new Rect2((Vector2)origin, (Vector2)size), new Color("0FFFFF00"));
        }
    }
}