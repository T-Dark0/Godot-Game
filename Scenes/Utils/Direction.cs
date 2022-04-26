public enum Direction
{
    Up,
    UpLeft,
    Left,
    DownLeft,
    Down,
    DownRight,
    Right,
    UpRight,
}

public static class DirectionMethods
{
    public static Direction? CombineDifferent(this Direction dir, Direction other)
    {
        return (dir, other) switch
        {
            (Direction.Up, Direction.Left) or (Direction.Left, Direction.Up) => Direction.UpLeft,
            (Direction.Down, Direction.Left) or (Direction.Left, Direction.Down) => Direction.DownLeft,
            (Direction.Down, Direction.Right) or (Direction.Right, Direction.Down) => Direction.DownRight,
            (Direction.Up, Direction.Right) or (Direction.Right, Direction.Up) => Direction.UpRight,
            _ => null,
        };
    }

    public static bool IsLinear(this Direction dir)
    {
        return dir switch
        {
            Direction.Up or Direction.Left or Direction.Down or Direction.Right => true,
            _ => false,
        };
    }

    public static bool IsColinearWith(this Direction dir, Direction other)
    {
        return (dir, other) switch
        {
            (Direction.Up or Direction.Down, Direction.Up or Direction.Down)
            or (Direction.Left or Direction.Right, Direction.Left or Direction.Right) => true,
            _ => false,
        };
    }
}