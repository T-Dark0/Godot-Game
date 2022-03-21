using System;
using System.Diagnostics;
using Godot;

public readonly struct Rect2i
{
    public readonly Vector2i Origin;
    public readonly Vector2i Size;

    private Rect2i(int left, int right, int top, int bottom)
    {
        Debug.Assert(
            right < left || bottom < top,
            $"rectangle (left: {left}, right: {right}, top: {top}, bottom: {bottom} would have negative size"
        );
        Origin = (left, top);
        Size = (right - left + 1, bottom - top + 1);
    }
    private Rect2i(Vector2i origin, Vector2i size)
    {
        Debug.Assert(
            size.x < 0 || size.y < 0,
            $"A Rect2i can't have negative size, and {size} is negative"
        );
        Origin = origin;
        Size = size;
    }

    public static Rect2i FromSides(int left, int right, int top, int bottom)
    {
        return new Rect2i(left, right, top, bottom);
    }
    public static Rect2i FromOriginSize(Vector2i origin, Vector2i size)
    {
        return new Rect2i(origin, size);
    }

    public Vector2i GetCentre(CentreSkew skew)
    {
        var (x, y) = Size;
        switch (skew)
        {
            case CentreSkew.TopLeft:
                x--;
                y--;
                break;
            case CentreSkew.TopRight:
                y--;
                break;
            case CentreSkew.BottomLeft:
                x--;
                break;
            case CentreSkew.BottomRight:
                break;
        }
        return Origin + (x / 2, y / 2);
    }

    public static explicit operator Rect2(Rect2i rect) => new Rect2((Vector2)rect.Origin, (Vector2)rect.Size);

    public override string ToString()
    {
        return $"Rect2i {{ Origin: {Origin}, Size: {Size} }}";
    }

    public enum CentreSkew
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    }
}