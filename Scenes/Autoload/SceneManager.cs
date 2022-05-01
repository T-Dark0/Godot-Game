using Godot;

public class SceneManager : Node
{
#nullable disable
    public static SceneManager Instance { get; private set; }
    public Node CurrentScene { get; private set; }
#nullable enable

    public override void _Ready()
    {
        Instance = this;
        var root = GetTree().Root;
        CurrentScene = root.GetChild(root.GetChildCount() - 1);
    }

    public static void GotoScene(PackedScene nextScene)
    {
        Instance.CallDeferred(nameof(DeferredGotoMainMenu), nextScene);
    }

    private void DeferredGotoMainMenu(PackedScene nextScene)
    {
        CurrentScene.Free();

        CurrentScene = nextScene.Instance();
        GetTree().Root.AddChild(CurrentScene);
    }
}
