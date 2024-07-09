using Godot;
using System;

public partial class BonusBar : TextureProgressBar
{
	private TextureProgressBar bar;
	private Timer timer;
	private BonusType bonusType;
	private Action callback;

	private Color color;
	private int seconds;

	public BonusBar(BonusType bonusType, int seconds, Color color, Action callback)
	{
		this.bonusType = bonusType;
		this.seconds = seconds;
		this.color = color;
		this.callback = callback;
	}

	public override void _Ready()
	{
		timer = new Timer();
		timer.OneShot = true;
		timer.WaitTime = seconds;
		timer.Timeout += OnTimerTimeout;
		AddChild(timer);

		TintProgress = color;
		TextureProgress = (Texture2D)GD.Load("res://Assets/Arts/UI/BonusProgress.png");
		Position = new Vector2(-64, -64);
		Size = new Vector2(64, 64);
		Scale = new Vector2(2, 2);
		FillMode = (int)FillModeEnum.CounterClockwise;
		Step = 0.1;

		MaxValue = timer.WaitTime;
		Value = timer.WaitTime;

		timer.Start();
	}

	public override void _Process(double delta)
	{
		Value = timer.TimeLeft;
	}

	private void OnTimerTimeout()
	{
		callback();
	}

	public void IsNewPicked(BonusType bonusType, Player player)
	{
		if (this.bonusType == bonusType)
		{
			player.BonusPicked -= IsNewPicked;
			QueueFree();
		}
	}
}
