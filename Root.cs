using System;
using Godot;

public class Root : Node2D
{
    private TileMap _tilemap;
    private Player _player;
    private Map _map;

    public override void _Ready()
    {
        this._tilemap = (TileMap)GetNode("TileMap");
        this._player = (Player)GetNode("Player");
        this._map = WorldGenerator.Generate(Guid.NewGuid().GetHashCode(), Globals.MapSize, Globals.MapEdgeBoundary);
        AddChild(this._map);

        mapToTilemap();
    }

    public override void _Process(float delta)
    {
        OS.SetWindowTitle($"{_player.Coords}");
    }

    private void mapToTilemap()
    {
        for (int x = 0; x < Globals.MapSize; x++)
        {
            for (int y = 0; y < Globals.MapSize; y++)
            {
                var tile = _map[x, y];
                _tilemap.SetCell(x, y, (int)tile);
            }
        }
    }
}
