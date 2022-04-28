using System;
using System.Collections.Generic;

public class LineOfSight
{
    public static IEnumerable<Vector2i> Between(Vector2i from, Vector2i to) => BetweenInner(from, to, infinite: false);
    public static IEnumerable<Vector2i> Towards(Vector2i from, Vector2i to) => BetweenInner(from, to, infinite: true);

    //adapted from <https://stackoverflow.com/a/11683720>
    private static IEnumerable<Vector2i> BetweenInner(Vector2i from, Vector2i to, bool infinite)
    {
        var (x, y) = from;

        var dx = to.x - from.x;
        var dy = to.y - from.y;

        var diagonalDx = Math.Sign(dx);
        var diagonalDy = Math.Sign(dy);
        var alignedDx = diagonalDx;
        var alignedDy = 0;
        var longest = Math.Abs(dx);
        var shortest = Math.Abs(dy);
        if (shortest > longest)
        {
            (longest, shortest) = (shortest, longest);
            alignedDy = diagonalDy;
            alignedDx = 0;
        }

        var error = longest / 2;
        var steps = infinite ? int.MaxValue : longest;
        for (int i = 0; i <= steps; i++)
        {
            yield return (x, y);
            error += shortest;
            if (error < longest)
            {
                x += alignedDx;
                y += alignedDy;
            }
            else
            {
                error -= longest;
                x += diagonalDx;
                y += diagonalDy;
            }
        }
    }
}