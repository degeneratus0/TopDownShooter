using Godot;

public partial class EjectedShell : RigidBody2D
{
    public void Eject(Vector2 playerVelocity)
    {
        AngularVelocity = GD.RandRange(20, 80);
        LinearVelocity = playerVelocity * 100 + new Vector2(GD.RandRange(20, 100), 
            GD.RandRange(100, 150)).Rotated(Rotation);
    }

    public void OnSleepingStateChanged()
    {
        SetDeferred(RigidBody2D.PropertyName.Freeze, true);
    }
}
