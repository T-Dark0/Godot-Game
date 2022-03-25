using Godot;

public class Player : Area2D
{
    private const float ZoomStep = 1.2f;

#nullable disable //loaded in _Ready()
    private Camera2D _camera;
#nullable enable 
    private Vector2i _coords;
    public Vector2i Coords
    {
        get
        {
            return _coords;
        }
        private set
        {
            _coords = value;
            Position = (Vector2)value * Globals.TileSize + new Vector2(0.5f, 0.5f);
        }
    }

    public override void _Ready()
    {
        _camera = (Camera2D)GetNode("Camera2D");
        Coords = new Vector2i(128, 128);
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
