using System.Threading.Tasks;
using Godot;

public class Player : Entity
{
    private const float ZoomStep = 1.2f;

#nullable disable //loaded in _Ready()
    private Camera2D _camera;
#nullable enable 

    public const int VISION_RADIUS = 15;

    [Signal]
    private delegate void Input(InputEvent @event);

    public override void _Ready()
    {
        _camera = GetNode<Camera2D>("Camera2D");
        _camera.MakeCurrent();
    }

    public override async Task PlayTurn(Level level)
    {
        InputResult result = InputResult.Continue;
        do
        {
            var input = (InputEvent)(await ToSignal(this, nameof(Input)))[0];
            result = HandleMovement(input, level, result);
            result = await HandleSpellcast(input, level, result);
        } while (result == InputResult.Continue);
        level.RevealAround(Coords, VISION_RADIUS);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        EmitSignal(nameof(Input), @event);
        HandleZoom(@event);
    }

    private InputResult HandleMovement(InputEvent @event, Level level, InputResult result)
    {
        var speed = Godot.Input.IsActionPressed("move_run") ? 16 : 1;

        if (@event.IsActionPressed("move_right", allowEcho: true))
        {
            Actions.MoveEntity(level, this, Coords + Vector2i.Right * speed);
            return InputResult.EndTurn;
        }
        if (@event.IsActionPressed("move_left", allowEcho: true))
        {
            Actions.MoveEntity(level, this, Coords + Vector2i.Left * speed);
            return InputResult.EndTurn;
        }
        if (@event.IsActionPressed("move_up", allowEcho: true))
        {
            Actions.MoveEntity(level, this, Coords + Vector2i.Up * speed);
            return InputResult.EndTurn;
        }
        if (@event.IsActionPressed("move_down", allowEcho: true))
        {
            Actions.MoveEntity(level, this, Coords + Vector2i.Down * speed);
            return InputResult.EndTurn;
        }
        return result;
    }

    private async Task<InputResult> HandleSpellcast(InputEvent @event, Level level, InputResult result)
    {
        if (@event.IsActionPressed("light_arrow"))
        {
            var globalMouse = GetGlobalMousePosition();
            var arrow = LightArrow.Instance();
            await Actions.FireProjectile(level, arrow, GlobalPosition, globalMouse);
            return InputResult.EndTurn;
        }
        return result;
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

public enum InputResult
{
    Continue,
    EndTurn,
}