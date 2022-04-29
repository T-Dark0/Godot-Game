using System;
using Godot;

public class Camera : Camera2D
{
    [Export]
    public float MinZoom { get; private set; }
    [Export]
    public float MaxZoom { get; private set; }
    [Export]
    public float ZoomStep { get; private set; }


    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("zoom_in"))
        {
            var newZoom = Math.Max(MinZoom, Zoom.x / ZoomStep);
            Zoom = new Vector2(newZoom, newZoom);
        }
        else if (@event.IsActionPressed("zoom_out"))
        {
            var newZoom = Math.Min(MaxZoom, Zoom.x * ZoomStep);
            Zoom = new Vector2(newZoom, newZoom);
        }
    }
}
