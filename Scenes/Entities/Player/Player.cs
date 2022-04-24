using System.Threading.Tasks;
using Godot;

public class Player : Entity
{
    private const float ZOOM_STEP = 1.2f;
    private const uint BASE_HEALTH = 100;
    public const int VISION_RADIUS = 15;

#nullable disable //Initialized in _Ready()
    private Camera2D _camera;
#nullable enable 

    [Signal]
    private delegate void Input(InputEvent @event);

    public override void _Ready()
    {
        base._Ready();
        _camera = GetNode<Camera2D>("Camera");
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
        level.Map.RevealAround(Coords, VISION_RADIUS);
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
            var target = level.Map.TileAtGlobalCoords(GetGlobalMousePosition());
            await Actions.FireProjectile(level, LightArrowFactory.Instance, Coords, target);
            return InputResult.EndTurn;
        }
        return result;
    }

    private void HandleZoom(InputEvent @event)
    {
        if (@event.IsActionPressed("zoom_in"))
        {
            _camera.Zoom /= ZOOM_STEP;
        }
        else if (@event.IsActionPressed("zoom_out"))
        {
            _camera.Zoom *= ZOOM_STEP;
        }
    }
}

public enum InputResult
{
    Continue,
    EndTurn,
}