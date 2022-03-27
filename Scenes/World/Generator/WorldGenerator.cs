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

        //for each room, select a random other room the path to which isn't obstructed by some third room
        var paths = graph.Nodes()
            .Select(from => graph.Nodes()
                .Select(to => (fromRoom: from, toRoom: to))
                .Where(pair =>
                    pair.fromRoom.id != pair.toRoom.id
                    && !IsRoomBetween(pair.fromRoom.data, pair.toRoom.data, graph)
                )
                .Random(rng)
            );
        //and add that path to the graph
        foreach (var ((fromId, fromRoom), (toId, toRoom)) in paths)
        {
            var fromCentre = GetRoomCentre(fromRoom);
            var toCentre = GetRoomCentre(toRoom);
            graph.AddEdge(fromId, toId, pathFactory.Create(fromCentre, toCentre));
        }
        GD.Print(graph);
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
        GD.PrintRaw($"\tpoint: {point}, rect. {rectangle}, side: {side}\n");
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

static class RandomFromEnumerableExtension
{
    /// <summary>
    /// Reservoir sampling, specialized for the case in which we only want to extract a single element.
    /// Adapted from https://en.wikipedia.org/wiki/Reservoir_sampling
    /// </summary>
    public static T? Random<T>(this IEnumerable<T> iter, Random rng)
    {
        var consumed = 0;
        var result = default(T);
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
}