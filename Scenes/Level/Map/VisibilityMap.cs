using Godot;

namespace GameMap;

public class VisibilityMap : TileMap
{
    public VisibilityTile this[Vector2i coords]
    {
        get { return (VisibilityTile)GetCell(coords.x, coords.y); }
        set { SetCell(coords.x, coords.y, (int)value); }
    }

    public void Initialize(WorldMap map)
    {
        foreach (var coord in map.TileCoords())
        {
            this[coord] = VisibilityTile.Black;
        }
    }

    public void RevealAround(WorldMap map, Vector2i viewpoint, int radius)
    {
        //Apply "revealed but not currently visible" fog of war to all tiles
        foreach (var coord in map.TileCoords())
        {
            if (this[coord] == VisibilityTile.Empty)
            {
                this[coord] = VisibilityTile.Fogged;
            }
        }
        //Then reveal the tiles that are actually visible
        foreach (var coord in FieldOfView.OfViewpoint(new WorldMapView(map), viewpoint, radius))
        {
            this[coord] = VisibilityTile.Empty;
        }
    }
}

public enum VisibilityTile
{
    Empty = -1,
    Black = 0,
    Fogged = 1,
}
