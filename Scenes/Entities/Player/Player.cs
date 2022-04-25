using System.Threading.Tasks;
using Godot;

public class Player : Entity
{
    private const float ZOOM_STEP = 1.2f;
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
        GD.Print(Coords);
        while (true)
        {
            var input = (InputEvent)(await ToSignal(this, nameof(Input)))[0];
            if (HandleMovement(input, level) == InputResult.EndTurn) break;
            if (await HandleSpellcast(input, level) == InputResult.EndTurn) break;
        }
        level.Map.RevealAround(Coords, VISION_RADIUS);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        EmitSignal(nameof(Input), @event);
        HandleZoom(@event);
    }

    private InputResult HandleMovement(InputEvent @event, Level level)
    {
        var speed = Godot.Input.IsActionPressed("move_run") ? 16 : 1;

        Vector2i direction;
        if (@event.IsActionPressed("move_right")) direction = Vector2i.Right;
        else if (@event.IsActionPressed("move_left")) direction = Vector2i.Left;
        else if (@event.IsActionPressed("move_up")) direction = Vector2i.Up;
        else if (@event.IsActionPressed("move_down")) direction = Vector2i.Down;
        else return InputResult.Continue;
        var result = Actions.MoveEntity(level, this, Coords + direction * speed);
        return result == ActionResult.Success ? InputResult.EndTurn : InputResult.Continue;
    }

    private async Task<InputResult> HandleSpellcast(InputEvent @event, Level level)
    {
        if (@event.IsActionPressed("light_arrow"))
        {
            var target = level.Map.TileAtGlobalCoords(GetGlobalMousePosition());
            await Actions.FireProjectile(level, LightArrowFactory.Instance, Coords, target);
            return InputResult.EndTurn;
        }
        return InputResult.Continue;
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