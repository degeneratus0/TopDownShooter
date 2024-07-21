using Godot;
using System;

public partial class BonusBar : TextureProgressBar
{
	public Bonus Bonus;

	private TextureProgressBar bar;
	private Timer timer;
	private Action callback;

	private int seconds;

	private static Vector2 position = new Vector2(-64, -64);
	private static Vector2 size = new Vector2(64, 64);
	private static Vector2 scale = new Vector2(2, 2);

	public BonusBar(Bonus bonus, int seconds, Action callback)
	{
		Bonus = bonus;
		this.seconds = seconds;
		this.callback = callback;
	}

	public override void _Ready()
	{
		timer = new Timer();
		timer.OneShot = true;
		timer.WaitTime = seconds;
		timer.Timeout += OnTimerTimeout;
		AddChild(timer);

		TintProgress = Bonus.Color;
		TextureProgress = (Texture2D)GD.Load("res://Assets/Arts/UI/BonusProgress.png");
		Position = position;
		Size = size;
		Scale = scale;
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

	public void Reset()
	{
		timer.Start(seconds);
	}

	private void OnTimerTimeout()
	{
		callback();
	}
}
