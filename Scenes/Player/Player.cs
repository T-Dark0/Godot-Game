using Godot;

public class Player : Sprite
{
    private const float ZoomStep = 1.2f;

#nullable disable //loaded in _Ready()
    private Camera2D _camera;
#nullable enable 
    private bool _running;

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
            Position = ((Vector2)value + new Vector2(0.5f, 0.5f)) * Globals.TileSize;
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
        if (@event.IsActionPressed("move_run"))
        {
            _running = true;
        }
        else if (@event.IsActionReleased("move_run"))
        {
            _running = false;
        }
        var speed = _running ? 16 : 1;

        if (@event.IsActionPressed("move_right"))
        {
            Coords += Vector2i.Right * speed;
        }
        if (@event.IsActionPressed("move_left"))
        {
            Coords += Vector2i.Left * speed;
        }
        if (@event.IsActionPressed("move_up"))
        {
            Coords += Vector2i.Up * speed;
        }
        if (@event.IsActionPressed("move_down"))
        {
            Coords += Vector2i.Down * speed;
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
