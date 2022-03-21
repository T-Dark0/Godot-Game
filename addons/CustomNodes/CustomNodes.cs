#if TOOLS

using System;
using Godot;

[Tool]
public class CustomNodes : EditorPlugin
{
    public override void _EnterTree()
    {
        var script = GD.Load<Script>("Scenes/Player/Player.cs");
        var texture = GD.Load<Texture>("icon.png");
        AddCustomType("Player", "Area2D", script, texture);
    }

    public override void _ExitTree()
    {
        RemoveCustomType("Player");
    }
}

#endif