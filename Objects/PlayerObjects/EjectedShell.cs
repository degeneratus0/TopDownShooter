using Godot;
using System;

public class EjectedShell : RigidBody2D
{
    public void Eject(Vector2 playersVelocity)
    {
        AngularVelocity = (float)GD.RandRange(20, 80);
        LinearVelocity = playersVelocity * 100 + new Vector2((float)GD.RandRange(20, 100), 
            (float)GD.RandRange(100, 150)).Rotated(Rotation);
    }
}
