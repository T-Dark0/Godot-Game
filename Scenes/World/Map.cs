using System.Collections.Generic;
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

    static int[] colors = {
        unchecked((int)0xFFFFFFFF), //white
        unchecked((int)0xFF0000FF), //red
        unchecked((int)0x00FF00FF), //green
        unchecked((int)0x0000FFFF), //blue
        unchecked((int)0x00FFFFFF), //cyan
        unchecked((int)0xFF00FFFF), //purple
        unchecked((int)0xFFFF00FF), //yellow
    };

    public override void _Draw()
    {
        foreach (var (_, room) in Graph.Nodes())
        {
            var origin = room.BoundingBox.Origin * 16;
            var size = room.BoundingBox.Size * 16;

            var font = new Control().GetFont("font");
            DrawString(font, (Vector2)origin, room.ToString());
            DrawRect(new Rect2((Vector2)origin, (Vector2)size), new Color("0FFFFF00"));
        }
        var color = 0;
        foreach (var (_, path) in Graph.Edges())
        {
            for (int i = 0; i < path.Points.Count - 1; i++)
            {
                var from = (Vector2)path.Points[i] * Globals.TileSize;
                var to = (Vector2)path.Points[i + 1] * Globals.TileSize;
                DrawLine(from, to, new Color(colors[color]));
            }
            color = (color + 1) % colors.Length;
        }
    }
}