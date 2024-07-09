using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class MobManager : Node
{
	private double soundTime = 0;
	private double minSoundTime = 5;
	private double maxSoundTime = 20;

	private int liveZombiesCount = 0;
	private int spawnedZombies = 0;
	private int spawnsCount;

	private int wave = 0;
	private int waveZombiesCount = GlobalSettings.Difficulty.WaveBase;
	private int currentWaveZombies = 0;

	private Timer zombieTimer;
	private Node zombieDeathSounds;
	private Node zombieAmbientSounds;

	private Player player;
	private Level level;
	private UI ui;

	private PackedScene ZombieScene;
	private PackedScene BloodScene;

	public override void _Ready()
	{
		zombieTimer = GetNode<Timer>("ZombieTimer");
		zombieDeathSounds = GetNode<Node>("ZombieDeathSounds");
		zombieAmbientSounds = GetNode<Node>("ZombieAmbientSounds");

		ZombieScene = GD.Load<PackedScene>("res://Entities/Zombie/Zombie.tscn");
		BloodScene = GD.Load<PackedScene>("res://Entities/Zombie/ZombieObjects/Blood.tscn");

		soundTime = GD.RandRange(minSoundTime, maxSoundTime);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (soundTime <= 0 && liveZombiesCount > 0)
		{
			minSoundTime = 500 / liveZombiesCount;
			maxSoundTime = 1000 / liveZombiesCount;
			soundTime = GD.RandRange(minSoundTime, maxSoundTime);
			zombieAmbientSounds.GetChild<AudioStreamPlayer>(Utilities.RandNum(zombieAmbientSounds.GetChildCount())).Play();
		}

		if (!GetTree().Paused)
		{
			soundTime -= delta;
		}
	}

	public void Init(Player player, Level level, UI ui)
	{
		this.player = player;
		this.level = level;
		this.ui = ui;
	}

	public void OnZombieTimerTimeout()
	{
		if (currentWaveZombies < waveZombiesCount)
		{
			List<Spawner> spawners = level.GetSpawners(player.GlobalPosition);
			if (!spawners.Any())
			{
				return;
			}

			Zombie zombie = (Zombie)ZombieScene.Instantiate();
			zombie.Position = spawners[GD.RandRange(0, spawners.Count - 1)].Position;
			zombie.SetPlayer(player);

			zombie.ZombieKilled += OnZombieKilled;

			AddChild(zombie);

			ui.UpdateZombikov(++liveZombiesCount);
			currentWaveZombies++;
		}
		else if (liveZombiesCount < 1)
		{
			wave++;
			waveZombiesCount = (int)(waveZombiesCount * GlobalSettings.Difficulty.WaveMultiplier);
			currentWaveZombies = 0;
		}
	}

	private void OnZombieKilled(Vector2 position, bool dropped)
	{
		AudioStreamPlayer audio = (AudioStreamPlayer)zombieDeathSounds.GetChild<AudioStreamPlayer>(Utilities.RandNum(zombieDeathSounds.GetChildCount())).Duplicate();
		audio.Autoplay = true;
		audio.Finished += audio.QueueFree;

		AddChild(audio);

		ui.UpdateZombikov(--liveZombiesCount);
		ui.UpdateScoreLabel(1);
		if (dropped)
		{
			level.AddBonus(Bonus.GetRandomBonus(), position);
		}
		Blood blood = (Blood)BloodScene.Instantiate();
		blood.Position = position;
		CallDeferred(Node.MethodName.AddChild, blood);
	}
}
