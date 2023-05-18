using Godot;

public class ShotSound : AudioStreamPlayer2D
{
    public void OnShotSoundFinished() => QueueFree();
}
