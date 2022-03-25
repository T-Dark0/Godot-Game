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

        CreateRoomGraph(rng, size, edgeBoundary, map);
    }

    private static void CreateRoomGraph(Random rng, int size, int edgeBoundary, Map map)
    {
        var graph = map.Graph;

        //Generate all the rooms
        var roomFactory = new Room.Factory(rng);
        foreach (var location in RoomLocations(rng, size, edgeBoundary))
        {
            graph.AddNode(roomFactory.Create(location));
        }

        var connectedRooms = graph.Nodes()
            .SelectMany(fromPair =>
            {
                var closestRoomDistance = graph.Nodes()
                    .Where(toPair => toPair.id != fromPair.id)
                    .Select(toPair => RoomDistance(fromPair.data, toPair.data))
                    .Min();
                var distanceThreshold = (int)(closestRoomDistance * (rng.NextDouble() * 0.2 + 1.2));
                var roomsInRange = graph.Nodes()
                    .Where(
                        toPair => toPair.id != fromPair.id
                        && RoomDistance(fromPair.data, toPair.data) < distanceThreshold
                    );
                return RepeatForever(fromPair).Zip(roomsInRange, (from, to) => (from, to));
            });
        var pathFactory = new Path.RecursiveBisectFactory(rng.Next());
        foreach (var ((fromId, fromRoom), (toId, toRoom)) in connectedRooms)
        {
            var fromCentre = fromRoom.BoundingBox.GetCentre(CentreSkew.BottomRight);
            var toCentre = toRoom.BoundingBox.GetCentre(CentreSkew.BottomRight);
            //The graph is supposed to be undirected, so don't insert the same edge twice in opposite directions
            var from = fromId;
            var to = toId;
            if (from < to)
            {
                var temp = from;
                from = to;
                to = temp;
            }
            //TODO: currently this check is necessary to avoid parallel edges. This may become unnecessary in the future if 
            //the graph class gains a method do do it
            if (!graph.ContainsEdge(from, to))
            {
                graph.AddEdge(from, to, pathFactory.Create(fromCentre, toCentre));
            }
        }
        GD.Print(graph);
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

    private static IEnumerable<T> RepeatForever<T>(T elem)
    {
        while (true) yield return elem;
    }

    private static float RoomDistance(Room from, Room to)
    {
        var fromCentre = from.BoundingBox.GetCentre(CentreSkew.BottomRight);
        var toCentre = to.BoundingBox.GetCentre(CentreSkew.BottomRight);
        return fromCentre.Distance(toCentre);
    }
}