using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class WorldGenerator : Reference
{
    private const int CandidateCount = 100;
    private const float Threshold = 0.45f;

    public static Map Generate(int seed, int size, int edgeBoundary)
    {
        var map = Map.Filled(size, size, Tile.Empty);
        var rng = new System.Random(seed);
        var roomNoise = new OpenSimplexNoise();
        roomNoise.Seed = seed;

        var locations = RoomLocations(rng, size, edgeBoundary);
        var roomFinder = RoomFinder(roomNoise, Threshold);
        foreach (var location in locations)
        {
            PlaceRoom(map, roomFinder, location, roomNoise, Threshold);
        }
        foreach (var location in locations)
        {
            map[location] = Tile.DebugRed;
        }
        var p1 = locations[rng.Next(0, locations.Count)];
        rng.Next(0, locations.Count); //TODO: remove
        var p2 = locations[rng.Next(0, locations.Count)];

        var tunnelNoise = new OpenSimplexNoise();
        tunnelNoise.Seed = seed;
        tunnelNoise.Octaves = 4;

        var path = map.AStar(
            p1, p2,
            Map.TaxicabDistance,
            (_x, _y) => 1
        );
        foreach (var point in path)
        {
            map[point] = Tile.DebugRed;
        }

        return map;
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

    private static void PlaceRoom(
        Map map,
        IEnumerator<float> roomFinder,
        Vector2i location,
        OpenSimplexNoise noise,
        float threshold
    )
    {
        roomFinder.MoveNext();
        var noiseOffset = roomFinder.Current;
        var queue = new Queue<Vector2i>();
        queue.Enqueue(location);
        while (queue.Count != 0)
        {
            var current = queue.Dequeue();
            if (!map.IsInBounds(current))
            {
                continue;
            }
            var tile = map[current];
            (float noiseX, float noiseY) = current - location;
            noiseX = noiseX + noiseOffset + 0.5f;
            noiseY = noiseY + 0.5f;
            var sample = noise.GetNoise2d(noiseX, noiseY);
            if (sample > threshold && tile != Tile.Floor)
            {
                map[current] = Tile.Floor;
                queue.Enqueue(current + Vector2i.Left);
                queue.Enqueue(current + Vector2i.Right);
                queue.Enqueue(current + Vector2i.Up);
                queue.Enqueue(current + Vector2i.Down);
            }
            else if (tile == Tile.Empty)
            {
                map[current] = Tile.Wall;
            }
        }
    }

    private static IEnumerator<float> RoomFinder(OpenSimplexNoise noise, float threshold)
    {
        var offset = 0.5f;
        while (true)
        {
            offset++;
            if (noise.GetNoise2d(offset, 0.5f) > threshold)
            {
                var roomStart = offset;
                while (noise.GetNoise2d(offset, 0.5f) > threshold)
                {
                    offset++;
                }
                var roomEnd = offset - 1;
                var roomCenter = (roomEnd + roomStart) / 2;
                yield return roomCenter;
            }
        }
    }

}