using Godot;
using System;

public class HPBar : Node2D
{
    private TextureProgress progress;

    public override void _Ready()
    {
        progress = GetNode<TextureProgress>("TextureProgress");
    }

    public override void _Process(float delta)
    {
        GlobalRotation = 0;
    }

    public void Init(int value)
    {
        progress.MaxValue = value;
        progress.Value = value;
    }

    public void SetValue(int value)
    {
        progress.Value = value;
    }
}
