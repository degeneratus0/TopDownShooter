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
	private int zombieDeathSoundsCount;
	private Node zombieAmbientSounds;
	private int zombieAmbientSoundsCount;

	private Player player;
	private Level level;
	private UI ui;

	private PackedScene BloodScene;

	public override void _Ready()
	{
		zombieTimer = GetNode<Timer>("ZombieTimer");
		zombieDeathSounds = GetNode<Node>("ZombieDeathSounds");
		zombieAmbientSounds = GetNode<Node>("ZombieAmbientSounds");

		BloodScene = GD.Load<PackedScene>("res://Entities/Zombie/ZombieObjects/Blood.tscn");

		soundTime = GD.RandRange(minSoundTime, maxSoundTime);

		zombieDeathSoundsCount = zombieDeathSounds.GetChildCount();
		zombieAmbientSoundsCount = zombieAmbientSounds.GetChildCount();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (soundTime <= 0 && liveZombiesCount > 0)
		{
			minSoundTime = 500 / liveZombiesCount;
			maxSoundTime = 1000 / liveZombiesCount;
			soundTime = GD.RandRange(minSoundTime, maxSoundTime);
			zombieAmbientSounds.GetChild<AudioStreamPlayer>(Utilities.RandNum(zombieAmbientSoundsCount - 1)).Play();
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

			Zombie zombie = ScenePool<Zombie>.Take();
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

	private void OnZombieKilled(Zombie zombie, bool dropped)
	{
		zombie.ZombieKilled -= OnZombieKilled;

		AudioStreamPlayer audio = (AudioStreamPlayer)zombieDeathSounds.GetChild<AudioStreamPlayer>(Utilities.RandNum(zombieDeathSoundsCount - 1)).Duplicate();
		audio.Autoplay = true;
		audio.Finished += audio.QueueFree;

		AddChild(audio);

		ui.UpdateZombikov(--liveZombiesCount);
		ui.UpdateScoreLabel(1);
		if (dropped)
		{
			level.AddBonus(Bonus.GetRandomBonus(), zombie.Position);
		}
		Blood blood = (Blood)BloodScene.Instantiate();
		blood.Position = zombie.Position;
		CallDeferred(Node.MethodName.AddChild, blood);
	}
}
