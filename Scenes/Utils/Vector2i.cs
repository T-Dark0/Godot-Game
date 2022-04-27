using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public readonly struct Vector2i
{
    public readonly int x;
    public readonly int y;

    public static Vector2i Zero = new Vector2i(0, 0);
    public static Vector2i Up = new Vector2i(0, -1);
    public static Vector2i UpLeft = new Vector2i(-1, -1);
    public static Vector2i Left = new Vector2i(-1, 0);
    public static Vector2i DownLeft = new Vector2i(-1, 1);
    public static Vector2i Down = new Vector2i(0, 1);
    public static Vector2i DownRight = new Vector2i(1, 1);
    public static Vector2i Right = new Vector2i(1, 0);
    public static Vector2i UpRight = new Vector2i(1, -1);

    public static List<Vector2i> NeighborOffsets = new List<Vector2i>
    {
        Up, UpLeft, Left, DownLeft, Down, DownRight, Right, UpRight,
    };

    public Vector2i(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void Deconstruct(out int x, out int y)
    {
        x = this.x;
        y = this.y;
    }

    public double DistanceTo(Vector2i other)
    {
        var dx = x - other.x;
        var dy = y - other.y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public static IEnumerable<Vector2i> NeighborsOf(Vector2i coord) => NeighborOffsets.Select(offset => coord + offset);
    public bool IsNeighborOf(Vector2i other) => (other - this).IsOffset();
    public bool IsOffset() => (Direction?)this is Direction;

    public static implicit operator Vector2i((int, int) tuple) => new Vector2i(tuple.Item1, tuple.Item2);

    public static explicit operator Vector2(Vector2i vec) => new Vector2(vec.x, vec.y);
    public static explicit operator Vector2i(Vector2 vec) => new Vector2i((int)vec.x, (int)vec.y);

    public static explicit operator Vector2i(Direction direction) => direction switch
    {
        Direction.Up => Vector2i.Up,
        Direction.UpLeft => Vector2i.UpLeft,
        Direction.Left => Vector2i.Left,
        Direction.DownLeft => Vector2i.DownLeft,
        Direction.Down => Vector2i.Down,
        Direction.DownRight => Vector2i.DownRight,
        Direction.Right => Vector2i.Right,
        Direction.UpRight => Vector2i.UpRight,
        _ => throw new ArgumentException(),
    };
    public static explicit operator Direction?(Vector2i vec) => vec switch
    {
        (0, -1) => Direction.Up,
        (-1, -1) => Direction.UpLeft,
        (-1, 0) => Direction.Left,
        (-1, 1) => Direction.DownLeft,
        (0, 1) => Direction.Down,
        (1, 1) => Direction.DownRight,
        (1, 0) => Direction.Right,
        (1, -1) => Direction.UpRight,
        _ => null,
    };

    public static Vector2i operator +(Vector2i lhs, Vector2i rhs)
    {
        return new Vector2i(lhs.x + rhs.x, lhs.y + rhs.y);
    }

    public static Vector2i operator -(Vector2i lhs, Vector2i rhs)
    {
        return new Vector2i(lhs.x - rhs.x, lhs.y - rhs.y);
    }

    public static Vector2i operator *(Vector2i lhs, int rhs)
    {
        return new Vector2i(lhs.x * rhs, lhs.y * rhs);
    }

    public static bool operator ==(Vector2i lhs, Vector2i rhs)
    {
        return lhs.x == rhs.x && lhs.y == rhs.y;
    }

    public static bool operator !=(Vector2i lhs, Vector2i rhs)
    {
        return !(lhs == rhs);
    }

    public override bool Equals(object obj)
    {
        return obj is Vector2i v && this == v;
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode();
    }

    public override string ToString()
    {
        return $"({x}, {y})";
    }
}