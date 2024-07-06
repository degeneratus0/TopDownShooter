using Godot;

public partial class BarrelFire : GpuParticles2D
{
    public override void _Ready()
    {
        GetNode<GpuParticles2D>("AdditionalFire").Emitting = true;
        Emitting = true;
    }
}
