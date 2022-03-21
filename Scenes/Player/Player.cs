using Godot;

public class Player : Area2D
{
    private const float ZoomStep = 1.2f;

    private Camera2D _camera;
    private Vector2i _coords;
    public Vector2i Coords
    {
        get
        {
            return _coords;
        }
        private set
        {
            //FIXME: This should be a "move" method: we need to maintain the half-tile offset
            _coords = value;
            Position = (Vector2)value * Globals.TileSize;
        }
    }

    public override void _Ready()
    {
        _camera = (Camera2D)GetNode("Camera2D");

        Position = Position.Snapped(Vector2.One * Globals.TileSize);
        Position += Vector2.One * Globals.TileSize / 2;
        _camera.MakeCurrent();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        HandleZoom(@event);
        HandleMovement(@event);
    }

    private void HandleMovement(InputEvent @event)
    {
        if (@event.IsActionPressed("move_right"))
        {
            Coords += Vector2i.Right;
        }
        if (@event.IsActionPressed("move_left"))
        {
            Coords += Vector2i.Left;
        }
        if (@event.IsActionPressed("move_up"))
        {
            Coords += Vector2i.Up;
        }
        if (@event.IsActionPressed("move_down"))
        {
            Coords += Vector2i.Down;
        }
    }

    private void HandleZoom(InputEvent @event)
    {
        if (@event.IsActionPressed("zoom_in"))
        {
            _camera.Zoom /= ZoomStep;
        }
        else if (@event.IsActionPressed("zoom_out"))
        {
            _camera.Zoom *= ZoomStep;
        }
    }
}
