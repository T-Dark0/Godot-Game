using System.Threading.Tasks;
using Godot;

public class DeathScreen : CanvasLayer
{
#nullable disable //Initialized in _Ready
    private AnimationPlayer _animation;
#nullable enable

    public override void _Ready()
    {
        _animation = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public async Task FadeIn()
    {
        _animation.Play("FadeIn");
        await ToSignal(_animation, "animation_finished");
    }
}
