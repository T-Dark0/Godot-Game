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
        Test();

        var rng = new Random(seed);
        var roomNoise = new OpenSimplexNoise();
        roomNoise.Seed = seed;

        var locations = RoomLocations(rng, size, edgeBoundary);
        var roomFactory = new Room.Factory(rng);
        foreach (var location in locations)
        {
            var room = roomFactory.Create(location);
            room.ApplyToMap(map);
            var centre = room.BoundingBox.GetCentre(CentreSkew.BottomRight);
            map[centre] = Tile.DebugGreen;
        }
        foreach (var location in locations)
        {
            map[location] = Tile.DebugRed;
        }
    }

    private static void Test()
    {
        var graph = new Graph<char, int>();
        var a = graph.AddNode('a');
        var b = graph.AddNode('b');
        var c = graph.AddNode('c');
        var d = graph.AddNode('d');
        graph.AddEdge(1, a, b);
        graph.AddEdge(2, a, c);
        graph.AddEdge(3, c, d);
        graph.AddEdge(4, a, d);
        graph.AddEdge(5, b, b);
        graph.AddEdge(6, d, a);

        GD.Print(graph);

        foreach (var (id, data) in graph.Neighbors(a))
        {
            GD.Print(data);
        }
    }

    /*
    private static RoomGraph CreateRoomGraph(Random rng, int size, int edgeBoundary)
    {
        var roomGraph = new RoomGraph();

        var roomFactory = new Room.Factory(rng);
        foreach (var location in RoomLocations(rng, size, edgeBoundary))
        {
            roomGraph.AddNode(roomFactory.Create(location));
        }

        Func<((NodeId id, Room room), (NodeId id, Room room)), float> distance = pair =>
       {
           var ((_, from), (_, to)) = pair;
           var fromCentre = from.BoundingBox.GetCentre(CentreSkew.BottomRight);
           var toCentre = to.BoundingBox.GetCentre(CentreSkew.BottomRight);
           return fromCentre.Distance(toCentre);
       };
        var closestRoomDistance = roomGraph.Nodes()
            .Combinations2()
            .Select(distance)
            .Min();
        var distanceThreshold = (int)(closestRoomDistance + rng.NextDouble() * 0.2 + 0.2);
        var roomsInRange = roomGraph.Nodes()
            .Combinations2()
            .Where(pair => distance(pair) < distanceThreshold);
        foreach (var ((from, _), (to, _)) in roomsInRange)
        {
            roomGraph.AddEdge(from, to);
        }

        return roomGraph
    }
    */

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
}

static class LinqExtension
{
    public static IEnumerable<(T, T)> Combinations2<T>(this IEnumerable<T> iter)
    {
        var counter = 0;
        foreach (var right in iter)
        {
            foreach (var left in iter.Take(counter))
            {
                yield return (left, right);
            }
            counter++;
        }
    }
}