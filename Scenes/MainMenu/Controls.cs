using Godot;

public class Controls : Control
{
#nullable disable //Initialized in _Ready
    private Sprite _cursor;
    private Button _backButton;
#nullable enable

    [Signal]
    public delegate void Back();

    public override void _Ready()
    {
        _cursor = GetNode<Sprite>("Cursor");
        _backButton = GetNode<Button>("BackButton");
        _backButton.GrabFocus();

        var x = _backButton.RectGlobalPosition.x + _backButton.RectSize.x / 2;
        var y = _backButton.RectGlobalPosition.y;
        _cursor.GlobalPosition = new Vector2(x, y);
    }

    public void OnBackButtonUp()
    {
        EmitSignal(nameof(Back));
    }
}
