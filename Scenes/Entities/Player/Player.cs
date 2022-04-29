using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class Player : Entity
{
    private const float ZOOM_STEP = 1.2f;
    private const uint WALK_SPEED = 5;
    private const uint RUN_SPEED = 12;
    public const int VISION_RADIUS = 15;

    private static Texture _trajectoryReachable = GD.Load<Texture>("res://Images/TrajectoryReachable.png");
    private static Texture _trajectoryBlocker = GD.Load<Texture>("res://Images/TrajectoryBlocker.png");
    private static Texture _trajectoryUnreachable = GD.Load<Texture>("res://Images/TrajectoryUnreachable.png");


#nullable disable //Initialized in _Ready()
    private Camera2D _camera;
    private Timer _timer;
#nullable enable

    [Signal]
    private delegate void InputReceived();

    public override void _Ready()
    {
        base._Ready();
        _camera = GetNode<Camera2D>("Camera");
        _camera.MakeCurrent();
        _timer = GetNode<Timer>("Timer");
    }

    public new void Initialize(Level level, Vector2i coords)
    {
        base.Initialize(level, coords);
        Visible = true;
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

        if (vec == Vector2i.Zero) return InputResult.Continue;
        return (axisAlignedPressed, diagonalPressed) switch
        {
            (1, 0) or (2, 0) or (0, 1) => await MovementHelper(level, vec, Input.IsActionPressed("move_run")),
            _ => InputResult.Continue,
        };
    }

    private async Task<InputResult> MovementHelper(Level level, Vector2i destination, bool run)
    {
        var moveResult = await MoveByOffset(level, destination, run ? RUN_SPEED : WALK_SPEED);
        if (moveResult == MoveResult.Success)
        {
            level.Map.RevealAround(Coords, VISION_RADIUS);
            EmitSignal(nameof(Moved), this);
            return InputResult.EndTurn;
        }
        else
        {
            return InputResult.Continue;
        }
    }

    private async Task<InputResult> HandleSpellcast(Level level)
    {
        if (Input.IsActionJustPressed("light_arrow"))
        {
            var target = level.Map.TileAtGlobalPosition(GetGlobalMousePosition());
            if (target == Coords) return InputResult.Continue;
            await Projectiles.SpawnLightArrow().Fire(level, Coords, target);
            return InputResult.EndTurn;
        }
        if (Input.IsActionJustPressed("preview_trajectory"))
        {
            var target = level.Map.TileAtGlobalPosition(GetGlobalMousePosition());
            var sprites = new List<Sprite>();
            var iter = Projectiles.LightArrow.Attributes
                .GetTrajectory(level, Coords, target)
                .TakeWhile(pair => level.Map.IsVisible(pair.Coord));
            foreach (var (coord, tile) in iter)
            {
                var sprite = new Sprite();
                sprite.Texture = tile switch
                {
                    TrajectoryTile.Reachable => _trajectoryReachable,
                    TrajectoryTile.Blocker => _trajectoryBlocker,
                    TrajectoryTile.Unreachable => _trajectoryUnreachable,
                    _ => throw new ArgumentException(),
                };
                sprite.GlobalPosition = level.Map.GlobalPositionOfTile(coord);
                sprites.Add(sprite);
                level.AddChild(sprite);
            }
            _timer.Start(0.5f);
            await ToSignal(_timer, "timeout");
            foreach (var sprite in sprites)
            {
                level.RemoveChild(sprite);
                sprite.QueueFree();
            }
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