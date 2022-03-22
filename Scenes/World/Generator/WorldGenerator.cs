using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class WorldGenerator : Reference
{
    private const int CandidateCount = 100;
    private const float Threshold = 0.45f;

    public static void Generate(int seed, int size, int edgeBoundary, Map map)
    {
        var rng = new System.Random(seed);
        var roomNoise = new OpenSimplexNoise();
        roomNoise.Seed = seed;

        var locations = RoomLocations(rng, size, edgeBoundary);

        var roomFactory = new Room.Factory(seed);
        foreach (var location in locations)
        {
            var room = roomFactory.Create(location);
            room.ApplyToMap(map);
            var centre = room.BoundingBox.GetCentre(Rect2i.CentreSkew.BottomRight);
            map[centre] = Tile.DebugGreen;
        }
        for (int i = 0; i < 2; i++)
        {
            new Path.MidpointOffsetFactory(rng).Create(locations[i], locations[i + 1]).ApplyToMap(map);
        }
        foreach (var location in locations)
        {
            map[location] = Tile.DebugRed;
        }
    }

    private static List<Vector2i> RoomLocations(Random rng, int mapSize, int mapEdgeBoundary)
    {
        var locations = new List<Vector2i>();
        var roomCount = rng.Next(5, 8);
        //var roomCount = 1;
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
        for (int i = 0; i < CandidateCount; i++)
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
        locations.Add(bestCandidate.Value);
    }
}