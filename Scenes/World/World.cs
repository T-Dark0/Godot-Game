using System.Collections.Generic;
using Godot;

public class Level : Node2D
{
    private Map _map;
    private List<Entity> _entities = new List<Entity>();
    private ulong _turn_counter = 0;

    private Level()
    {

    }

    public static void GenerateMap()
    { }

    public void PlayTurn()
    {
        foreach (var entity in _entities)
        {
            entity.PlayTurn(this);
        }
    }
}