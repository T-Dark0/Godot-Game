using System;
using Godot;

public class Root : Node2D
{
    private TileMap tilemap;
    private Map map;

    public override void _Ready()
    {
        this.tilemap = (TileMap)GetNode("TileMap");
        this.map = WorldGenerator.Generate(Guid.NewGuid().GetHashCode(), Globals.MapSize, Globals.MapEdgeBoundary);
        mapToTilemap();
    }

    private void mapToTilemap()
    {
        for (int x = 0; x < Globals.MapSize; x++)
        {
            for (int y = 0; y < Globals.MapSize; y++)
            {
                var tile = map[x, y];
                tilemap.SetCell(x, y, (int)tile);
            }
        }
    }
}
