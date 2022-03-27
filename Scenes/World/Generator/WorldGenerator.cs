using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class WorldGenerator
{
    private const int CANDIDATE_COUNT = 100;
    private const float THRESHOLD = 0.45f;

    public static void Generate(int seed, int size, int edgeBoundary, Map map)
    {
        var rng = new Random(seed);
        var roomNoise = new OpenSimplexNoise();
        roomNoise.Seed = seed;

        CreateRoomGraph(rng, size, edgeBoundary, map.Graph);
    }

    private static void CreateRoomGraph(Random rng, int size, int edgeBoundary, Graph<Room, Path> inGraph)
    {
        var graph = new UnionFindGraph<Room, Path>(inGraph);

        var roomFactory = new Room.Factory(rng);
        var pathFactory = new Path.RecursiveBisectFactory(rng.Next());

        //create all the rooms
        foreach (var location in RoomLocations(rng, size, edgeBoundary))
        {
            var fromRoom = roomFactory.Create(location);
            var fromId = graph.AddNode(fromRoom);
        }

        // To ensure connectedness, we build a spanning tree before adding more random edges (todo)
        // for each room, select a random other room:
        // - the path to which isn't obstructed by some third room
        // - which won't form a cycle if connected to this room
        // It's possible that a given room will fail to find any existing other room satisfying both constraints
        // In that case, we simply don't create an edge from that given room. A random room will pick up the slack later
        // Proof: (TODO: make this clearer, possibly move to somewhere other than here)
        // Consider that there is _always_ a reachable room ignoring cycle detection, because if a given room is obstructed,
        // then the obstruction itself is a reachable room
        // Then, the only reason we can fail to find a room to connect to is that all reachable rooms are
        // already in the same "group" of nodes.
        // This means that whichever node we would have connected to, had it not been for the obstruction, is still
        // not in our group of nodes, which in turn implies that its turn to be the current node will come later
        var paths = graph.Nodes()
            .SelectWhere(from => graph.Nodes()
                .Select(to => (fromRoom: from, toRoom: to))
                .Where(pair =>
                    !graph.WouldFormCycle(pair.fromRoom.id, pair.toRoom.id)
                    && !IsRoomBetween(pair.fromRoom.data, pair.toRoom.data, graph)
                )
                .Random(rng)
            );
        foreach (var ((fromId, fromRoom), (toId, toRoom)) in paths)
        {
            var fromCentre = GetRoomCentre(fromRoom);
            var toCentre = GetRoomCentre(toRoom);
            graph.AddEdge(fromId, toId, pathFactory.Create(fromCentre, toCentre));
        }
    }

    /// <summary>
    /// Simplified version of the Cohen–Sutherland algorithm, 
    /// taken from https://en.wikipedia.org/wiki/Cohen%E2%80%93Sutherland_algorithm<br/>
    /// In particular, since we don't care about where the intersection is, just that it exists,
    /// we don't compute the intersection point
    /// </summary>
    private static bool IsRoomBetween(Room from, Room to, UnionFindGraph<Room, Path> graph)
    {
        var fromPoint = GetRoomCentre(from);
        var toPoint = GetRoomCentre(to);
        return graph
            .Nodes()
            .Select(room => (CategorizeSide(fromPoint, room.data.BoundingBox), CategorizeSide(toPoint, room.data.BoundingBox)))
            .Where(pair => pair.Item1 != Side.Inside && pair.Item2 != Side.Inside)
            .Any(pair => (pair.Item1 & pair.Item2) == 0);
    }

    private static Side CategorizeSide(Vector2i point, Rect2i rectangle)
    {
        var side = Side.Inside;
        if (point.x < rectangle.Left) side |= Side.Left;
        else if (point.x > rectangle.Right) side |= Side.Right;
        if (point.y < rectangle.Top) side |= Side.Up;
        else if (point.y > rectangle.Bottom) side |= Side.Down;
        return side;
    }

    private static Vector2i GetRoomCentre(Room room)
    {
        return room.BoundingBox.GetCentre(CentreSkew.BottomRight);
    }

    private static List<Vector2i> RoomLocations(Random rng, int mapSize, int mapEdgeBoundary)
    {
        var locations = new List<Vector2i>();
        var roomCount = rng.Next(5, 8);
        for (int i = 0; i < roomCount; i++)
        {
            AddRoomLocation(locations, rng, mapSize, mapEdgeBoundary);
        }
        return locations;
    }

    private static void AddRoomLocation(List<Vector2i> locations, Random rng, int mapSize, int mapEdgeBoundary)
    {
        var min = mapEdgeBoundary;
        var max = mapSize - mapEdgeBoundary;

        Vector2i? bestCandidate = null;
        var bestDistance = 0.0;
        for (int i = 0; i < CANDIDATE_COUNT; i++)
        {
            var candidate = new Vector2i(rng.Next(min, max), rng.Next(min, max));
            var distance = locations
                .Select(location =>
                {
                    var dx = location.x - candidate.x;
                    var dy = location.y - candidate.y;
                    return Math.Sqrt(dx * dx + dy * dy) as double?;
                })
                .Min() ?? int.MaxValue;
            if (distance > bestDistance)
            {
                bestDistance = distance;
                bestCandidate = candidate;
            }
        }
        locations.Add(bestCandidate!.Value);
    }

    [Flags]
    private enum Side : byte
    {
        Inside = 0,
        Up = 0b0001,
        Down = 0b0010,
        Left = 0b0100,
        Right = 0b1000,
    }
}

// NOTE: the methods in this class are restricted to T: struct because C# behaves really awkwardly otherwise
// Specifically, it will make Random return a defaulted T instead of a null one
// and it will make SelectWhere return U? even though the declared return type is U
static class RandomFromEnumerableExtension
{
    /// <summary>
    /// Reservoir sampling, specialized for the case in which we only want to extract a single element.
    /// Adapted from https://en.wikipedia.org/wiki/Reservoir_sampling
    /// </summary>
    public static T? Random<T>(this IEnumerable<T> iter, Random rng) where T : struct
    {
        var consumed = 0;
        T? result = null;
        foreach (var elem in iter)
        {
            consumed++;
            if (rng.Next(consumed) == 0)
            {
                result = elem;
            }
        }
        return result;
    }

    /// <summary>
    /// Combined Select and Where: if the function returns an element, the resulting enumerable
    /// will contain that element, and if the function returns null, the element is dropped instead
    /// <summary>
    public static IEnumerable<U> SelectWhere<T, U>(this IEnumerable<T> iter, Func<T, U?> f) where U : struct
    {
        foreach (var elem in iter)
        {
            var mapped = f(elem);
            if (mapped is U u) yield return u;
        }
    }
}