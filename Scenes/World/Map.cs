using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Map : TileMap
{
    public List<Room> Rooms; //HACK: Remove
    public List<Path> Paths;
    public int width { get; private set; }
    public int height { get; private set; }

    private static readonly Vector2i[] NeighbourOffsets = new Vector2i[] { (-1, 0), (1, 0), (0, -1), (0, 1) };

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

    public List<Vector2i> AStar(
        Vector2i from,
        Vector2i goal,
        Func<Vector2i, Vector2i, float> heuristic,
        Func<Vector2i, Vector2i, float> stepCost
    )
    {
        var comparer = Comparer<float>.Create((x, y) => x.CompareTo(y));
        var frontier = new MinHeap<Vector2i, float>(comparer);
        frontier.Enqueue(from, 0);
        var cameFrom = new Dictionary<Vector2i, Vector2i>();
        var costUntil = new Dictionary<Vector2i, float>();
        costUntil.Add(from, 0);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            if (current == goal) break;
            foreach (var next in NeighboursOf(current))
            {
                var newCost = costUntil[current] + stepCost(current, next);
                if (!costUntil.ContainsKey(next) || newCost < costUntil[next])
                {
                    costUntil[next] = newCost;
                    var priority = newCost + heuristic(next, goal);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        var path = new List<Vector2i>();
        var current2 = goal;
        while (true)
        {
            path.Add(current2);
            if (!cameFrom.ContainsKey(current2)) break;
            current2 = cameFrom[current2];
        }
        path.Reverse();
        return path;
    }

    public static float TaxicabDistance(Vector2i p1, Vector2i p2)
    {
        return Math.Abs(p1.x - p2.x) + Math.Abs(p1.y - p2.y);
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

    private IEnumerable<Vector2i> NeighboursOf(Vector2i point)
    {
        return NeighbourOffsets
            .Select(offset => offset + point)
            .Where(p => IsInBounds(p));
    }
}