using Godot;

public class Player : Area2D
{
    private const float ZoomStep = 1.2f;

    private Camera2D _camera;
    private ObjectToMove _objectToMove = ObjectToMove.Player;

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
        HandleToggleControls(@event);
        HandleMovement(@event);
    }

    private void HandleToggleControls(InputEvent @event)
    {
        if (@event.IsActionPressed("toggle_player_camera_controls"))
        {
            switch (_objectToMove)
            {
                case ObjectToMove.Player:
                    _objectToMove = ObjectToMove.Camera;
                    break;
                case ObjectToMove.Camera:
                    _objectToMove = ObjectToMove.Player;
                    _camera.Position = Vector2.Zero;
                    break;
            }
        }
    }

    private void HandleMovement(InputEvent @event)
    {
        Node2D obj = null;
        switch (_objectToMove)
        {
            case ObjectToMove.Player:
                obj = this;
                break;
            case ObjectToMove.Camera:
                obj = _camera;
                break;
        }
        if (@event.IsActionPressed("move_right"))
        {
            obj.Position += Vector2.Right * Globals.TileSize;
        }
        if (@event.IsActionPressed("move_left"))
        {
            obj.Position += Vector2.Left * Globals.TileSize;
        }
        if (@event.IsActionPressed("move_up"))
        {
            obj.Position += Vector2.Up * Globals.TileSize;
        }
        if (@event.IsActionPressed("move_down"))
        {
            obj.Position += Vector2.Down * Globals.TileSize;
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

    private enum ObjectToMove
    {
        Camera,
        Player,
    }
}
