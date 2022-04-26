using System;
using System.Collections.Generic;

namespace GameMap;

// The methods and types in this class are adapted from https://www.albertford.com/shadowcasting/
public class FieldOfView
{
    public static void OfViewpoint(WorldMap map, Vector2i viewpoint, int radius, Action<Vector2i> reveal)
    {
        reveal(viewpoint);
        foreach (var quadrant in Quadrant.Quadrants(viewpoint))
        {
            Scan(map, quadrant, new Row(1, -1, 1), radius, reveal);
        }
    }

    private static void Scan(WorldMap map, Quadrant quadrant, Row row, int radius, Action<Vector2i> reveal)
    {
        QuadrantCoord? previousCoord = null;
        foreach (var currentCoord in row.Coords())
        {
            if (!IsInRange(currentCoord, radius))
            {
                continue;
            }
            // Walls are always visible if any part of their tile is, tiles are only visible if their center is in the area being
            // swept, so as to be symmetric
            if (BlocksLight(map, quadrant, currentCoord) || IsOriginVisibleFrom(row, currentCoord))
            {
                reveal(quadrant.ToGlobalCoords(currentCoord));
            }
            if (BlocksLight(map, quadrant, previousCoord) && AllowsLight(map, quadrant, currentCoord))
            {
                row.StartSlope = LeftSlope(currentCoord);
            }
            if (AllowsLight(map, quadrant, previousCoord) && BlocksLight(map, quadrant, currentCoord))
            {
                var next = row.Next();
                next.EndSlope = LeftSlope(currentCoord);
                Scan(map, quadrant, next, radius, reveal);
            }
            previousCoord = currentCoord;
        }
        if (AllowsLight(map, quadrant, previousCoord))
        {
            Scan(map, quadrant, row.Next(), radius, reveal);
        }
    }

    private static bool IsInRange(QuadrantCoord coord, int radius)
    {
        var (row, col) = coord;
        return (row * row) + (col * col) <= radius * radius;
    }

    private static bool BlocksLight(WorldMap map, Quadrant quadrant, QuadrantCoord? coord)
    {
        return coord is QuadrantCoord c && map[quadrant.ToGlobalCoords(c)].BlocksLight();
    }

    private static bool AllowsLight(WorldMap map, Quadrant quadrant, QuadrantCoord? coord)
    {
        return coord is QuadrantCoord c && !map[quadrant.ToGlobalCoords(c)].BlocksLight();
    }

    private static double LeftSlope(QuadrantCoord coord)
    {
        var (row_depth, col) = coord;
        return (double)(2 * col - 1) / (double)(2 * row_depth);
    }

    private static bool IsOriginVisibleFrom(Row row, QuadrantCoord coord)
    {
        var (row_depth, col) = coord;
        return col >= row.Depth * row.StartSlope && col <= row.Depth * row.EndSlope;
    }

    private readonly record struct Quadrant(Vector2i Origin, QuadrantId Id)
    {
        public static IEnumerable<Quadrant> Quadrants(Vector2i origin)
        {
            return new Quadrant[]
            {
                new Quadrant(origin, QuadrantId.Top),
                new Quadrant(origin, QuadrantId.Left),
                new Quadrant(origin, QuadrantId.Bottom),
                new Quadrant(origin, QuadrantId.Right),
            };
        }

        public Vector2i ToGlobalCoords(QuadrantCoord local)
        {
            var (row, col) = local;
            var (x, y) = Origin;
            return Id switch
            {
                QuadrantId.Top => (x + col, y - row),
                QuadrantId.Left => (x - row, y + col),
                QuadrantId.Bottom => (x + col, y + row),
                QuadrantId.Right => (x + row, y + col),
                _ => throw new ArgumentException() //_sigh_ exhaustive enums when, C#?
            };
        }
    }

    private enum QuadrantId
    {
        Top,
        Left,
        Bottom,
        Right,
    }

    private readonly record struct QuadrantCoord(int Row, int Col);

    private class Row
    {
        public int Depth;
        public double StartSlope;
        public double EndSlope;

        public Row(int depth, double startSlope, double endSlope)
        {
            Depth = depth;
            StartSlope = startSlope;
            EndSlope = endSlope;
        }

        public Row Next()
        {
            return new Row(Depth + 1, StartSlope, EndSlope);
        }

        public IEnumerable<QuadrantCoord> Coords()
        {
            var minCol = (int)Math.Floor(Depth * StartSlope + 0.5);
            var maxCol = (int)Math.Ceiling(Depth * EndSlope - 0.5);
            for (int i = minCol; i <= maxCol; i++)
            {
                yield return new QuadrantCoord(Depth, i);
            }
        }
    }
}