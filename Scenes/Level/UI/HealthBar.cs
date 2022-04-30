using Godot;

public class HealthBar : Control
{
#nullable disable
    [Export] private Texture BarGreen;
    [Export] private Texture BarYellow;
    [Export] private Texture BarRed;
    [Export] private double YellowThreshold;
    [Export] private double RedThreshold;
#nullable enable

#nullable disable //Initialized in _Ready()
    private TextureProgress _bar;
    private Label _label;
#nullable enable

    public override void _Ready()
    {
        _bar = GetNode<TextureProgress>("Bar");
        _label = GetNode<Label>("Label");
        Visible = false;
    }

    public void OnHealthChange(Health health)
    {
        var percent = (double)health.Current / (double)health.Max;

        if (percent == 1)
        {
            Visible = false;
            return;
        }
        _label.Text = $"{health.Current}/{health.Max}";

        _bar.TextureProgress_ = BarGreen;
        if (percent < YellowThreshold) _bar.TextureProgress_ = BarYellow;
        if (percent < RedThreshold) _bar.TextureProgress_ = BarRed;
        _bar.Value = percent * 100;

        Visible = true;
    }
}
