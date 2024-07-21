using Godot;

public partial class Blood : Node2D
{
	public override void _Ready()
	{
		Sprite2D bloodSprite = GetNode<Sprite2D>("BloodSplatter" + GD.RandRange(1, 2));
		bloodSprite.Rotate((float)GD.RandRange(-Mathf.Pi, Mathf.Pi));
		bloodSprite.Show();
		var particles = GetNode<GpuParticles2D>("GPUParticles2D");
		particles.Emitting = true;
		particles.Finished += particles.QueueFree;
	}
}
