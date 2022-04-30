using System;
using Godot;
using GodotArray = Godot.Collections.Array;

public class MainMenu : CanvasLayer
{
#nullable disable //Initialized in _Ready
    private Sprite _cursor;
    private VBoxContainer _buttons;
#nullable enable

    public override void _Ready()
    {
        _cursor = GetNode<Sprite>("Cursor");
        _buttons = GetNode<VBoxContainer>("Buttons");

        for (int i = 0; i < _buttons.GetChildCount(); i++)
        {
            var button = _buttons.GetChild(i);
            button.Connect("focus_entered", this, nameof(MoveCursorToButton), new GodotArray(button));
            button.Connect("mouse_entered", this, nameof(MoveCursorToButton), new GodotArray(button));
        }
        OnLoad();
    }

    private void OnLoad()
    {
        _buttons.GetChild<Button>(0).GrabFocus();
    }

    private async void OnNewGameButtonUp()
    {
        var root = GetNode("/root");
        root.RemoveChild(this);

        var level = Scenes.InstanceLevel();
        root.AddChild(level);
        level.Initialize(new Random());
        await level.PlayGame();
        level.QueueFree();

        root.RemoveChild(level);
        root.AddChild(this);
        OnLoad();
    }

    private void OnQuitButtonUp()
    {
        GetTree().Quit();
    }

    private void MoveCursorToButton(Button button)
    {
        button.GrabFocus();
        var x = button.RectGlobalPosition.x + button.RectSize.x / 2;
        var y = button.RectGlobalPosition.y;
        _cursor.GlobalPosition = new Vector2(x, y);
    }
}
