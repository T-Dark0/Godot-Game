using System;
using System.Collections.Generic;
using Godot;

public class Path
{
    public List<Vector2i> Points { get; private set; }

    private Path(List<Vector2i> points)
    {
        Points = points;
    }

    public class RecursiveBisectFactory
    {
        private Random _rng;

        public int MaxDepth = 6;
        public float OffsetLengthScale = 1.0f / 3.0f;

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
            return new Path(points);
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
    }
}