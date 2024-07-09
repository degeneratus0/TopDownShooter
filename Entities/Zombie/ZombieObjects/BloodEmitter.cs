using Godot;

public partial class BloodEmitter : GpuParticles2D
{
	public override void _Ready()
	{
		Emitting = true;
        Finished += QueueFree;
    }
}
