using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Path
{
    public List<Vector2i> Points { get; private set; }

    private Path(List<Vector2i> points)
    {
        Points = points;
    }

    /*
    public Path(Vector2i start, Vector2i end, int minSegmentLength, int maxSegmentLength, Random rng)
    {
        Points = new List<Vector2i>();
        Points.Add(start);
        var cumulatedLength = 0;
        var angle = (float)(rng.NextDouble() * Math.PI * 2); // [0, 2pi)
        while (cumulatedLength < totalLength)
        {
            var length = rng.Next(minSegmentLength, maxSegmentLength);
            cumulatedLength += length;

            var new_angle = (float)(rng.NextDouble() * Math.PI - (Math.PI / 2)); // [-pi/2, pi/2)
            angle += new_angle;
            var x = (int)(Math.Cos(angle) * length);
            var y = (int)(Math.Sin(angle) * length);

            var point = Points.Last();
            Points.Add(point + (x, y));
        }
    }
    */

    public void ApplyToMap(Map map)
    {
        map.Paths.Add(this);
    }

    public class MidpointOffsetFactory
    {
        private List<Vector2i> _points;
        private Random _rng;

        public MidpointOffsetFactory(Random rng)
        {
            _points = new List<Vector2i>();
            _rng = rng;
        }

        public Path Create(Vector2i from, Vector2i to)
        {
            CreatePoints((Vector2)from, (Vector2)to, 0);
            return new Path(_points);
        }

        private void CreatePoints(Vector2 from, Vector2 to, int depth)
        {
            if (depth > 5) return;
            var midpoint = newMidpoint(from, to);
            CreatePoints(from, midpoint, depth + 1);
            CreatePoints(midpoint, to, depth + 1);
            _points.Add((Vector2i)midpoint);
        }

        private Vector2 newMidpoint(Vector2 from, Vector2 to)
        {
            var dx = to.x - from.x;
            var dy = to.y - from.y;
            var fromToLength = Math.Sqrt(dx * dy + dy * dy);

            var midpoint = new Vector2(dx / 2, dy / 2);
            var newVecLength = (float)(_rng.NextDouble() * fromToLength / 3);
            var newVecDirection = new Vector2(dy, -dx).Normalized();
            return midpoint + newVecDirection * newVecLength;
        }
    }
}