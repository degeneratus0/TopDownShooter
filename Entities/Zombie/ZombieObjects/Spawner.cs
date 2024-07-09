using Godot;

public partial class Spawner : Marker2D
{
	public VisibleOnScreenNotifier2D VisibilityNotifier;

	public override void _Ready()
	{
		VisibilityNotifier = GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreen");
	}
}
