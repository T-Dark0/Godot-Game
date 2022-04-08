using Godot;

public class VisibilityMap : TileMap
{
    public VisibilityTile this[Vector2i coords]
    {
        get { return (VisibilityTile)GetCell(coords.x, coords.y); }
        set { SetCell(coords.x, coords.y, (int)value); }
    }

    public void RevealAround(WorldMap map, Vector2i viewpoint)
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
}

public enum VisibilityTile
{
    Empty = -1,
    Black = 0,
}
