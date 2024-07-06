using Godot;

public partial class ShotSound : AudioStreamPlayer2D
{
    public void OnShotSoundFinished() => QueueFree();
}
