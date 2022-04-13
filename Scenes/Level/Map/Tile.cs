public enum Tile
{
    Empty = -1,
    Wall = 0,
    Floor = 1,
    DebugRed = 2,
    DebugGreen = 3,
}

public static class TileMethods
{
    public static bool BlocksLight(this Tile tile)
    {
        return tile == Tile.Wall || tile == Tile.Empty;
    }
}