using Godot;

public partial class BonusSpawner : Marker2D
{
	[Export] public PackedScene Bonus;
	[Export] public int SecondsToSpawn = 15;

	private Timer timer;
	private TextureProgressBar progressBar;
	private Bonus instance;

	public override void _Ready()
	{
		timer = GetNode<Timer>("Timer");
		progressBar = GetNode<TextureProgressBar>("TextureProgressBar");

		timer.WaitTime = SecondsToSpawn;
		timer.Start();

		instance = (Bonus)Bonus.Instantiate();

		progressBar.TintProgress = instance.Color;
		progressBar.Step = 0.5;
		progressBar.MaxValue = timer.WaitTime;
		progressBar.Value = timer.WaitTime;
	}

	public override void _Process(double delta)
	{
		progressBar.Value = timer.TimeLeft;
	}

	private void OnTimerTimeout()
	{
		AddChild(instance);
		instance.Picked += (_) => timer.Start();
		instance = (Bonus)Bonus.Instantiate();
	}
}
