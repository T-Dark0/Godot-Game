using System;
using System.Collections.Generic;
using Godot;

public class Room
{
    public Dictionary<Vector2i, Tile> Tiles { get; private set; }
    public Rect2i BoundingBox { get; private set; }

    private Room() { }

    //FIXME: Don't let walls replace floors
    public void ApplyToMap(Map map)
    {
        map.Rooms.Add(this);
        foreach (var tile in Tiles)
        {
            map[tile.Key] = tile.Value;
        }
    }

    public class Factory
    {
        private IEnumerator<float> _roomFinder;
        private OpenSimplexNoise _noise;
        private float _threshold;

        public Factory(int seed)
        {
            var noise = new OpenSimplexNoise();
            noise.Seed = seed;
            _noise = noise;
            _threshold = 0.45f;
            _roomFinder = RoomFinder();
        }

        public Room Create(Vector2i origin)
        {
            var tiles = new Dictionary<Vector2i, Tile>();
            var left = int.MaxValue;
            var right = int.MinValue;
            var top = int.MaxValue;
            var bottom = int.MinValue;

            _roomFinder.MoveNext();
            var noiseOffset = _roomFinder.Current;
            var queue = new Queue<Vector2i>();
            queue.Enqueue(origin);
            while (queue.Count != 0)
            {
                var current = queue.Dequeue();

                left = Math.Min(left, current.x);
                right = Math.Max(right, current.x);
                top = Math.Min(top, current.y);
                bottom = Math.Max(bottom, current.y);

                var noiseCoords = current - origin;
                var noiseX = noiseCoords.x + noiseOffset + 0.5f;
                var noiseY = noiseCoords.y + 0.5f;
                var sample = _noise.GetNoise2d(noiseX, noiseY);
                Tile tile;
                if (!tiles.TryGetValue(current, out tile))
                {
                    tile = Tile.Empty;
                }
                if (sample > _threshold && tile != Tile.Floor)
                {
                    tiles[current] = Tile.Floor;
                    queue.Enqueue(current + Vector2i.Left);
                    queue.Enqueue(current + Vector2i.Right);
                    queue.Enqueue(current + Vector2i.Up);
                    queue.Enqueue(current + Vector2i.Down);
                }
                else if (tile == Tile.Empty)
                {
                    tiles[current] = Tile.Wall;
                }
            }

            var room = new Room();
            room.Tiles = tiles;
            room.BoundingBox = Rect2i.FromSides(left, right, top, bottom);
            return room;
        }

        private IEnumerator<float> RoomFinder()
        {
            var offset = 0.5f;
            while (true)
            {
                offset++;
                if (_noise.GetNoise2d(offset, 0.5f) > _threshold)
                {
                    var roomStart = offset;
                    while (_noise.GetNoise2d(offset, 0.5f) > _threshold)
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
}