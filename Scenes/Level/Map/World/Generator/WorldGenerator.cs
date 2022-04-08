using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class WorldGenerator
{
    private const int CANDIDATE_COUNT = 100;
    private const float THRESHOLD = 0.45f;
    private const int MIN_ROOM_COUNT = 5;
    private const int MAX_ROOM_COUNT = 8;
    private const int MAP_SIZE = 128;

    public static void Generate(Random rng, WorldMap map)
    {
        var roomNoise = new OpenSimplexNoise();
        roomNoise.Seed = rng.Next();

        CreateRoomGraph(rng, map.Graph);
        GraphToMap(map);
    }

    private static void CreateRoomGraph(Random rng, Graph<Room, Path> inGraph)
    {
        var graph = new UnionFindGraph<Room, Path>(inGraph);

        var roomFactory = new Room.Factory(rng);

        //create all the rooms
        foreach (var location in RoomLocations(rng))
        {
            var fromRoom = roomFactory.Create(location);
            var fromId = graph.AddNode(fromRoom);
        }
        CreateMinSpanningTree(rng, graph);
    }

    private static List<Vector2i> RoomLocations(Random rng)
    {
        var locations = new List<Vector2i>();
        var roomCount = rng.Next(MIN_ROOM_COUNT, MAX_ROOM_COUNT);
        for (int i = 0; i < roomCount; i++)
        {
            AddRoomLocation(locations, rng);
        }
        return locations;
    }

    private static void AddRoomLocation(List<Vector2i> locations, Random rng)
    {
        Vector2i? bestCandidate = null;
        var bestDistance = 0.0;
        for (int i = 0; i < CANDIDATE_COUNT; i++)
        {
            var candidate = new Vector2i(rng.Next(MAP_SIZE), rng.Next(MAP_SIZE));
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

    // To ensure that the graph is fully connected, we first begin a minimum spanning tree before adding more edges
    // randomly to make it more interesting.
    // Building an MST per se wouldn't be terribly difficult. However, we have to work under a second constraint as well:
    // a room A may not connect to a room B if there exists a room C between them, blocking the way
    // 
    // Given a room A, there always is at least one other room that it can reach without a third room being in the way:
    // if there _is_ a room in the way, then that room, not the originally intended one, is a room that A can reach
    // However, there doesn't have to always be a room that isn't yet connected to A. (in fact, in an MST implementation
    // without the "no rooms in the way" constraint, every node would be able to find another node to connect to, except for the
    // very last node visited).
    // In our case, it's possible for a room that _isn't_ the last to be unable to find another room to connect to,
    // because all reachable candidates are already connected to it
    // In that case, we simply discard the room and don't bother connecting it to anything
    // This causes the algorithm to "fall behind" by one edge, so to speak
    // However, the graph is still guaranteed to be connected: A later room, which would have been unable to find anyone
    // not yet connected to it otherwise, will be able to make a connection
    private static void CreateMinSpanningTree(Random rng, UnionFindGraph<Room, Path> graph)
    {
        var pathFactory = new Path.RecursiveBisectFactory(rng.Next());

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

    // Simplified version of the Cohen–Sutherland algorithm, 
    // taken from https://en.wikipedia.org/wiki/Cohen%E2%80%93Sutherland_algorithm<br/>
    // In particular, since we don't care about where the intersection is, just that it exists,
    // we don't compute the intersection point
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

    private static Vector2i GetRoomCentre(Room room)
    {
        return room.BoundingBox.GetCentre(CentreSkew.BottomRight);
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

    [Flags]
    private enum Side : byte
    {
        Inside = 0,
        Up = 0b0001,
        Down = 0b0010,
        Left = 0b0100,
        Right = 0b1000,
    }

    private static void GraphToMap(WorldMap map)
    {
        foreach (var (_, room) in map.Graph.Nodes())
        {
            foreach (var (coord, tile) in room.Tiles())
            {
                SetTile(map, coord, tile);
            }
        }
        foreach (var (_, path) in map.Graph.Edges())
        {
            foreach (var (coord, tile) in path.Tiles())
            {
                SetTile(map, coord, tile);
            }
        }
    }

    private static void SetTile(WorldMap map, Vector2i coord, Tile tile)
    {
        switch (tile)
        {
            case Tile.Wall:
                if (map[coord] == Tile.Empty)
                {
                    map[coord] = Tile.Wall;
                }
                break;
            case Tile.Floor:
                var current = map[coord];
                if (current == Tile.Empty || current == Tile.Wall)
                {
                    map[coord] = Tile.Floor;
                }
                break;
            default:
                map[coord] = tile;
                break;
        }
    }
}
