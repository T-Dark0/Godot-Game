using System;
using System.Collections.Generic;
using Godot;

public class WorldMap : TileMap
{
    public Graph<Room, Path> Graph;

    private WorldMap()
    {
        Graph = new Graph<Room, Path>();
    }

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

    public Vector2i GetTileCoordsAt(Vector2 globalCoords)
    {
        return (Vector2i)WorldToMap(ToLocal(globalCoords));
    }

    public IEnumerable<Vector2i> TileCoords()
    {
        foreach (Vector2 coord in GetUsedCells())
        {
            yield return (Vector2i)coord;
        }
    }

    public List<Vector2i> AStar(
        Vector2i from,
        Vector2i goal,
        Func<Vector2i, Vector2i, float> heuristic,
        Func<Vector2i, IEnumerable<Vector2i>> neighboursOf,
        Func<Vector2i, Vector2i, float> stepCost
    )
    {
        var frontier = new MinHeap<Vector2i, float>();
        frontier.Enqueue(from, 0);
        var cameFrom = new Dictionary<Vector2i, Vector2i>();
        var costUntil = new Dictionary<Vector2i, float>();
        costUntil.Add(from, 0);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            if (current == goal) break;
            foreach (var next in neighboursOf(current))
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
}