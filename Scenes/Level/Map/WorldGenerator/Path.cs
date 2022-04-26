using System;
using System.Collections.Generic;
using System.Linq;
using GameMap;
using Godot;

namespace WorldGenerator;

public class Path
{
    private List<Vector2i> _points;
    private Dictionary<Vector2i, Tile> _tiles;

    public IEnumerable<Vector2i> Points()
    {
        return _points;
    }

    public IEnumerable<(Vector2i Coords, Tile Tile)> Tiles()
    {
        return _tiles.Select(pair => (pair.Key, pair.Value));
    }

    private Path(List<Vector2i> points, Dictionary<Vector2i, Tile> tiles)
    {
        _points = points;
        _tiles = tiles;
    }

    public class RecursiveBisectFactory
    {
        private Random _rng;

        public int MaxDepth = 6;
        public float OffsetLengthScale = 1.0f / 3.0f;
        public int CircleRadiusMin = 3;
        public int CircleRadiusMax = 5;

        public RecursiveBisectFactory(int seed)
        {
            _rng = new Random(seed);
        }

        public Path Create(Vector2i from, Vector2i to)
        {
            var points = new List<Vector2i>();
            points.Add(from);
            CreatePoints(points, (Vector2)from, (Vector2)to, 0);
            points.Add(to);

            var tiles = new Dictionary<Vector2i, Tile>();

            foreach (var point in points)
            {
                GenerateCircle(point, tiles);
            }
            return new Path(points, tiles);
        }

        private void CreatePoints(List<Vector2i> points, Vector2 from, Vector2 to, int depth)
        {
            if (depth > MaxDepth) return;
            var point = newPoint(from, to);
            CreatePoints(points, from, point, depth + 1);
            points.Add((Vector2i)point);
            CreatePoints(points, point, to, depth + 1);
        }

        private Vector2 newPoint(Vector2 from, Vector2 to)
        {
            var dx = to.x - from.x;
            var dy = to.y - from.y;
            var fromToLength = Math.Sqrt(dx * dx + dy * dy);

            var offsetLength = (float)((_rng.NextDouble() - 0.5) * 2 * fromToLength * OffsetLengthScale);
            var offsetDirection = new Vector2(dy, -dx).Normalized();
            var offset = offsetDirection * offsetLength;
            var midpoint = new Vector2((to.x + from.x) / 2, (to.y + from.y) / 2);
            var newPoint = midpoint + offset;
            return newPoint;
        }

        //Variant of Bresenham's circle algorithm.
        //Adapted from http://www.sunshine2k.de/coding/java/Bresenham/RasterisingLinesCircles.pdf
        private void GenerateCircle(Vector2i center, Dictionary<Vector2i, Tile> tiles)
        {
            var radius = _rng.Next(CircleRadiusMin, CircleRadiusMax + 1);
            var x = 0;
            var y = radius;
            var d = 1 - radius;
            var movedVertically = false;
            while (x < y)
            {
                DrawLine(tiles, center, -x, x, y, fill: movedVertically);
                DrawLine(tiles, center, -x, x, -y, fill: movedVertically);
                DrawLine(tiles, center, -y, y, x);
                DrawLine(tiles, center, -y, y, -x);
                if (d < 0)
                {
                    d = d + 2 * x + 3;
                    x += 1;
                    movedVertically = false;
                }
                else
                {
                    d = d + 2 * (x - y) + 5;
                    x += 1;
                    y -= 1;
                    movedVertically = true;
                }
            }
        }

        private void DrawLine(
            Dictionary<Vector2i, Tile> tiles,
            Vector2i center,
            int xFrom,
            int xTo,
            int y,
            bool fill = true
        )
        {
            SetWall(tiles, (xFrom, y) + center);
            SetWall(tiles, (xTo, y) + center);
            if (fill)
            {
                for (var x = xFrom + 1; x < xTo; x++)
                {
                    SetFloor(tiles, (x, y) + center);
                }
            }
        }

        private void SetWall(Dictionary<Vector2i, Tile> tiles, Vector2i point)
        {
            if (!tiles.ContainsKey(point))
            {
                tiles[point] = Tile.Wall;
            }
        }
        private void SetFloor(Dictionary<Vector2i, Tile> tiles, Vector2i point)
        {
            Tile tile;
            if (!tiles.TryGetValue(point, out tile) || tile == Tile.Wall)
            {
                tiles[point] = Tile.Floor;
            }
        }
    }
}