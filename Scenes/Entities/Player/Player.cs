using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class Player : Entity
{
    [Export]
    public uint WalkSpeed { get; private set; }
    [Export]
    public uint RunSpeed { get; private set; }
    [Export]
    public uint CameraWalkSpeed { get; private set; }
    [Export]
    public uint CameraRunSpeed { get; private set; }
    [Export]
    public int VisionRadius { get; private set; }

    private static Texture _trajectoryReachable = GD.Load<Texture>("res://Images/TrajectoryReachable.png");
    private static Texture _trajectoryBlocker = GD.Load<Texture>("res://Images/TrajectoryBlocker.png");
    private static Texture _trajectoryUnreachable = GD.Load<Texture>("res://Images/TrajectoryUnreachable.png");

    private static readonly (string, Vector2i)[] _axisAlignedActions = {
        ("move_up", Vector2i.Up), ("move_left", Vector2i.Left), ("move_down", Vector2i.Down), ("move_right", Vector2i.Right)
    };
    private static readonly (string, Vector2i)[] _diagonalActions = {
        ("move_up_left", Vector2i.UpLeft), ("move_down_left", Vector2i.DownLeft),
        ("move_down_right", Vector2i.DownRight), ("move_up_right", Vector2i.UpRight),
    };

#nullable disable //Initialized in _Ready()
    private Camera2D _camera;
    private Timer _timer;
#nullable enable
    private ToMove _toMove = ToMove.Player;

    [Signal]
    private delegate void InputReceived();

    public override void _Ready()
    {
        base._Ready();
        _camera = GetNode<Camera2D>("Camera");
        _camera.MakeCurrent();
        _timer = GetNode<Timer>("Timer");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        EmitSignal(nameof(InputReceived));
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
            var result = await HandleMovement(level);
            if (result == HandlerResult.EndTurn) break;
            else if (result == HandlerResult.Restart) continue;

            result = await HandleSpellcast(level);
            if (result == HandlerResult.EndTurn) break;
            else if (result == HandlerResult.Restart) continue;

            await ToSignal(this, nameof(InputReceived));
        }
    }

    private async Task<HandlerResult> HandleMovement(Level level)
    {
        if (Input.IsActionJustPressed("toggle_camera_control"))
        {
            if (_toMove == ToMove.Player) _toMove = ToMove.Camera;
            else
            {
                _toMove = ToMove.Player;
                _camera.Offset = Vector2.Zero;
            }
        }

        Movement movement;
        if (GetMovement() is Movement m) movement = m;
        else return HandlerResult.NextHandler;
        var (offset, run) = movement;
        if (_toMove == ToMove.Player) return await MovePlayer(level, offset, run);
        else return await MoveCamera(offset, run);
    }

    private Movement? GetMovement()
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

        if (vec == Vector2i.Zero) return null;
        return (axisAlignedPressed, diagonalPressed) switch
        {
            (1, 0) or (2, 0) or (0, 1) => new Movement(vec, Input.IsActionPressed("move_run")),
            _ => null,
        };
    }

    private async Task<HandlerResult> MovePlayer(Level level, Vector2i offset, bool run)
    {
        var moveResult = await MoveByOffset(level, offset, run ? RunSpeed : WalkSpeed);
        if (moveResult == MoveResult.Success)
        {
            level.Map.RevealAround(Coords, VisionRadius);
            EmitSignal(nameof(Moved), this);
            return HandlerResult.EndTurn;
        }
        else
        {
            return HandlerResult.NextHandler;
        }
    }

    private async Task<HandlerResult> MoveCamera(Vector2i offset, bool run)
    {
        var pixelOffset = (Vector2)(offset * Globals.TILE_SIZE);
        var time = 1.0f / (run ? CameraRunSpeed : CameraWalkSpeed);
        Tween.InterpolateProperty(_camera, "offset", null, _camera.Offset + pixelOffset, time);
        Tween.Start();
        await ToSignal(Tween, "tween_completed");
        return HandlerResult.Restart;
    }

    private async Task<HandlerResult> HandleSpellcast(Level level)
    {
        if (Input.IsActionJustPressed("light_arrow"))
        {
            var target = level.Map.TileAtGlobalPosition(GetGlobalMousePosition());
            if (target == Coords) return HandlerResult.NextHandler;
            await Projectiles.SpawnLightArrow().Fire(level, Coords, target);
            return HandlerResult.EndTurn;
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

        return HandlerResult.NextHandler;
    }

    private readonly record struct Movement(Vector2i offset, bool run);

    private enum ToMove
    {
        Player,
        Camera
    }
}

public enum HandlerResult
{
    Restart,
    NextHandler,
    EndTurn,
}