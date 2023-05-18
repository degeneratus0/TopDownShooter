using Godot;
using System;

public class Blood : Node2D
{
    private Particles2D particles;

    public override void _Ready()
	{
        particles = GetNode<Particles2D>("Particles2D");
		Sprite sprite = GetNode<Sprite>("BloodSplatter");

		sprite.Rotate((float)GD.RandRange(-Mathf.Pi, Mathf.Pi));
        particles.Emitting = true;
    }
}
