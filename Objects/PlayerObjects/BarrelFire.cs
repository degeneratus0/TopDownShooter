using Godot;
using System;

public class BarrelFire : Particles2D
{
    public override void _Ready()
    {
        GetNode<Particles2D>("AdditionalFire").Emitting = true;
        Emitting = true;
    }
}
