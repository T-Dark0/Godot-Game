using System;
using System.Collections.Generic;
using Godot;

public class VisibilityMap : TileMap
{
    public VisibilityTile this[Vector2i coords]
    {
        get { return (VisibilityTile)GetCell(coords.x, coords.y); }
        set { SetCell(coords.x, coords.y, (int)value); }
    }

    public void Initialize(WorldMap map)
    {
        BlackOut(map);
    }

    public void BlackOut(WorldMap map)
    {
        foreach (var coord in map.TileCoords())
        {
            this[coord] = VisibilityTile.Black;
        }
    }


    // This method, as well as many of the other methods and types it's implemented in terms of,
    // are adapted from https://www.albertford.com/shadowcasting/
    public void RevealAround(WorldMap map, Vector2i viewpoint, int radius)
    {
        UpdateFogOfWar(map);
        this[viewpoint] = VisibilityTile.Empty;

        foreach (var quadrant in Quadrant.Quadrants(viewpoint))
        {
            Scan(map, quadrant, new Row(1, -1, 1), radius);
        }
    }

    private void UpdateFogOfWar(WorldMap map)
    {
        foreach (var coord in map.TileCoords())
        {
            if (this[coord] == VisibilityTile.Empty)
            {
                this[coord] = VisibilityTile.Fogged;
            }
        }
    }

    private void Scan(WorldMap map, Quadrant quadrant, Row row, int radius)
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
                Reveal(quadrant, currentCoord);
            }
            if (BlocksLight(map, quadrant, previousCoord) && AllowsLight(map, quadrant, currentCoord))
            {
                row.StartSlope = LeftSlope(currentCoord);
            }
            if (AllowsLight(map, quadrant, previousCoord) && BlocksLight(map, quadrant, currentCoord))
            {
                var next = row.Next();
                next.EndSlope = LeftSlope(currentCoord);
                Scan(map, quadrant, next, radius);
            }
            previousCoord = currentCoord;
        }
        if (AllowsLight(map, quadrant, previousCoord))
        {
            Scan(map, quadrant, row.Next(), radius);
        }
    }

    private bool IsInRange(QuadrantCoord coord, int radius)
    {
        var (row, col) = coord;
        return (row * row) + (col * col) <= radius * radius;
    }

    private void Reveal(Quadrant quadrant, QuadrantCoord coord)
    {
        this[quadrant.ToGlobalCoords(coord)] = VisibilityTile.Empty;
    }

    private bool BlocksLight(WorldMap map, Quadrant quadrant, QuadrantCoord? coord)
    {
        return coord is QuadrantCoord c && map[quadrant.ToGlobalCoords(c)].BlocksLight();
    }

    private bool AllowsLight(WorldMap map, Quadrant quadrant, QuadrantCoord? coord)
    {
        return coord is QuadrantCoord c && !map[quadrant.ToGlobalCoords(c)].BlocksLight();
    }

    private double LeftSlope(QuadrantCoord coord)
    {
        var (row_depth, col) = coord;
        return (double)(2 * col - 1) / (double)(2 * row_depth);
    }

    private bool IsOriginVisibleFrom(Row row, QuadrantCoord coord)
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

public enum VisibilityTile
{
    Empty = -1,
    Black = 0,
    Fogged = 1,
}
