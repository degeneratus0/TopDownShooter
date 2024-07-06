using Godot;

public partial class HPBar : TextureProgressBar
{
    public void Init(int value)
    {
        MaxValue = value;
        Value = value;
    }

    public void SetValue(int value)
    {
        Value = value;
    }
}
