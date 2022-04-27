using System.Threading.Tasks;
using Godot;

public class Player : Entity
{
    private const float ZOOM_STEP = 1.2f;
    private const uint WALK_SPEED = 5;
    private const uint RUN_SPEED = 12;
    public const int VISION_RADIUS = 15;

#nullable disable //Initialized in _Ready()
    private Camera2D _camera;
#nullable enable

    [Signal]
    private delegate void InputReceived();

    public override void _Ready()
    {
        base._Ready();
        _camera = GetNode<Camera2D>("Camera");
        _camera.MakeCurrent();
    }

    public override async Task PlayTurn(Level level)
    {
        while (true)
        {
            if (await HandleMovement(level) == InputResult.EndTurn) break;
            if (await HandleSpellcast(level) == InputResult.EndTurn) break;
            await ToSignal(this, nameof(InputReceived));
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        EmitSignal(nameof(InputReceived));
        HandleZoom(@event);
    }

    private static readonly (string, Vector2i)[] _axisAlignedActions = {
        ("move_up", Vector2i.Up), ("move_left", Vector2i.Left), ("move_down", Vector2i.Down), ("move_right", Vector2i.Right)
    };
    private static readonly (string, Vector2i)[] _diagonalActions = {
        ("move_up_left", Vector2i.UpLeft), ("move_down_left", Vector2i.DownLeft),
        ("move_down_right", Vector2i.DownRight), ("move_up_right", Vector2i.UpRight),
    };
    private async Task<InputResult> HandleMovement(Level level)
    {
        var vec = Vector2i.Zero;
        var axisAlignedPressed = 0;
        foreach (var (action, dir) in _axisAlignedActions)
        {
            if (Input.IsActionPressed(action))
            {
                vec += dir;
                axisAlignedPressed++;
            }
        }
        var diagonalPressed = 0;
        foreach (var (action, dir) in _diagonalActions)
        {
            if (Input.IsActionPressed(action))
            {
                vec += dir;
                diagonalPressed++;
            }
        }

        return (axisAlignedPressed, diagonalPressed) switch
        {
            (1, 0) or (2, 0) or (0, 1) => await MovementHelper(level, vec, Input.IsActionPressed("move_run")),
            _ => InputResult.Continue,
        };
    }

    private async Task<InputResult> MovementHelper(Level level, Vector2i vec, bool run)
    {
        GD.Print($"MovementHelper: vec:{vec}, run:{run}");
        var direction = (Direction?)vec;

        var moveResult = await Move(level, direction!.Value, run ? RUN_SPEED : WALK_SPEED);
        if (moveResult == MoveResult.Success)
        {
            level.Map.RevealAround(Coords, VISION_RADIUS);
            return InputResult.EndTurn;
        }
        else
        {
            return InputResult.Continue;
        }
    }

    private async Task<InputResult> HandleSpellcast(Level level)
    {
        if (Input.IsActionPressed("light_arrow"))
        {
            var target = level.Map.TileAtGlobalPosition(GetGlobalMousePosition());
            await Projectiles.LightArrow().Fire(level, Coords, target);
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