using Godot;

public abstract class Entity : Sprite
{
    private Vector2i _coords;
    public Vector2i Coords
    {
        get
        {
            return _coords;
        }
        set
        {
            _coords = value;
            Position = ((Vector2)value + new Vector2(0.5f, 0.5f)) * Globals.TILE_SIZE;
        }
    }

    public void Initialize(Vector2i coords)
    {
        Coords = coords;
    }
    public void EndTurn()
    {
        EmitSignal(nameof(Level.PassTurn));
    }

    public virtual void OnTurnStart() { }
    public virtual void Input(InputEvent @event) { }
}
