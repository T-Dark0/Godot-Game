using System;
using Godot;

public readonly struct Vector2i
{
    public readonly int x;
    public readonly int y;

    public static Vector2i Up = new Vector2i(0, -1);
    public static Vector2i Down = new Vector2i(0, 1);
    public static Vector2i Left = new Vector2i(-1, 0);
    public static Vector2i Right = new Vector2i(1, 0);

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

    public static implicit operator Vector2i((int, int) tuple) => new Vector2i(tuple.Item1, tuple.Item2);

    public static explicit operator Vector2(Vector2i vec) => new Vector2(vec.x, vec.y);
    public static explicit operator Vector2i(Vector2 vec) => new Vector2i((int)vec.x, (int)vec.y);

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