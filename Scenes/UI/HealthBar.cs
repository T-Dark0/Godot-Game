using Godot;

public class HealthBar : Control
{
    private static Texture _barGreen = GD.Load<Texture>("res://Images/HealthBarGreen.png");
    private static Texture _barYellow = GD.Load<Texture>("res://Images/HealthBarYellow.png");
    private static Texture _barRed = GD.Load<Texture>("res://Images/HealthBarRed.png");

    private const double YELLOW_THRESHOLD = 0.75;
    private const double RED_THRESHOLD = 0.25;

#nullable disable //Initialized in _Ready()
    private TextureProgress _bar;
#nullable enable

    public override void _Ready()
    {
        _bar = GetNode<TextureProgress>("Bar");
        Visible = false;
    }

    public void OnHealthChange(Health health)
    {
        var percent = (double)health.Current / (double)health.Max;

        if (percent == 1) Visible = false;
        else Visible = true;

        _bar.TextureProgress_ = _barGreen;
        if (percent < YELLOW_THRESHOLD) _bar.TextureProgress_ = _barYellow;
        if (percent < RED_THRESHOLD) _bar.TextureProgress_ = _barRed;
        _bar.Value = percent * 100;
    }
}
