using Godot;

public partial class BarrelFire : GpuParticles2D
{
	public override void _Ready()
	{
		GpuParticles2D additionalFire = GetNode<GpuParticles2D>("AdditionalFire");
		Emitting = true;
		additionalFire.Emitting = true;

		Finished += QueueFree;
	}
}
