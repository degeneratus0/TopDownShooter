using Godot;
using System;

public class BloodParticles : Particles2D
{
    public override void _Ready() => Emitting = true;
}
