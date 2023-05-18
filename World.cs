using Godot;
using shootem.Objects.Bonuses;
using System;
using System.Collections.Generic;

public class World : Node2D
{
    private int ZombiesToIncrement;

    private float zombieSpawnRate;
	private float zombieSpawnRateIncrement;
    private float maxSpawnRate;

    private int spawnedZombies = 0;
    private double minSoundTime = 2;
    private double maxSoundTime = 6;
    private double soundTime = 0;

    private Player player;
	private Timer zombieTimer;
	private UI ui;
	private Control deathScreen;
	private Control pauseScreen;
	private Label scoreLabel;
	private Node zombieAmbientSounds;
	private Node zombieDeathSounds;
	private AudioStreamPlayer playerDeathSound;
	private AudioStreamPlayer pickUpSound;

	private PackedScene ZombieScene;
	private PackedScene BloodScene;
	private List<PackedScene> BonusesScenes;

	public override void _Ready()
	{
		GD.Randomize();

		GetNodes();
		LoadScenes();
        InitWorld();

		player.Connect(nameof(Player.AmmoUpdated), ui, nameof(UI.UpdateAmmoLabel));

		zombieTimer.WaitTime = zombieSpawnRate;
		ui.UpdateSpawnRateLabel(zombieSpawnRate);
	}

	private void GetNodes()
	{
		player = GetNode<Player>("Player");
		zombieTimer = GetNode<Timer>("ZombieTimer");
		ui = GetNode<UI>("UI");
		deathScreen = GetNode<Control>("UI/DeathScreen");
		pauseScreen = GetNode<Control>("UI/PauseScreen");
		scoreLabel = GetNode<Label>("UI/ScoreLabel");
		zombieAmbientSounds = GetNode("ZombieAmbientSounds");
		zombieDeathSounds = GetNode<Node>("ZombieDeathSounds");
		playerDeathSound = GetNode<AudioStreamPlayer>("PlayerDeathSound");
		pickUpSound = GetNode<AudioStreamPlayer>("ClipPickUpSound");
	}

	private void LoadScenes()
    {
        ZombieScene = GD.Load<PackedScene>("res://Characters/Zombie.tscn");
        BloodScene = GD.Load<PackedScene>("res://Objects/Blood.tscn");

        BonusesScenes = new List<PackedScene>()
        {
            GD.Load<PackedScene>("res://Objects/Bonuses/BulletPack.tscn"),
            GD.Load<PackedScene>("res://Objects/Bonuses/Aid.tscn"),
            GD.Load<PackedScene>("res://Objects/Bonuses/DamageUp.tscn")
        };
    }

	public void InitWorld()
	{
		zombieSpawnRate = 1 / GlobalSettings.Difficulty.ZombieSpawnRate;
		zombieSpawnRateIncrement = (float)GlobalSettings.Difficulty.ZombieSpawnRateIncrement / 100;
		maxSpawnRate = 1 / GlobalSettings.Difficulty.MaxSpawnRate;
		ZombiesToIncrement = GlobalSettings.Difficulty.ZombiesToIncrement;
		ui.SpawnRateVisibility(GlobalSettings.Difficulty.ShowSpawnRate);
	}

	public override void _PhysicsProcess(float delta)
	{
		if (soundTime <= 0)
		{
			soundTime = GD.RandRange(minSoundTime, maxSoundTime);
			zombieAmbientSounds.GetChild<AudioStreamPlayer>(Utilities.RandNum(zombieAmbientSounds.GetChildCount())).Play();
		}
		soundTime -= delta;
        FreeOneShotEmitters();
    }

	public void NewGame()
	{
		QueueFree();
		GetTree().ReloadCurrentScene();
	}

	public void GameOver()
	{
		playerDeathSound.Play();
		deathScreen.Show();
		zombieTimer.Stop();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("reload") && deathScreen.Visible)
		{
			NewGame();
		}
		if (@event.IsActionPressed("quit"))
		{
			if (deathScreen.Visible)
            {
                GetTree().Paused = false;
                GetTree().ChangeScene("res://UI/MainMenu.tscn");
            }
			else if (!pauseScreen.Visible)
            {
                GetTree().Paused = true;
                pauseScreen.Show();
            }
			else if (pauseScreen.Visible)
            {
                GetTree().Paused = false;
                pauseScreen.Hide();
            }
        }
        if (@event.IsActionPressed("menu") && pauseScreen.Visible)
        {
            GetTree().Paused = false;
            GetTree().ChangeScene("res://UI/MainMenu.tscn");
        }
        if (@event.IsActionPressed("toggle_fullscreen"))
		{
			OS.WindowFullscreen = !OS.WindowFullscreen;
		}
	}

	public void OnZombieTimerTimeout()
	{
		if (zombieTimer.WaitTime > maxSpawnRate)
		{
			spawnedZombies++;
			if (spawnedZombies > ZombiesToIncrement)
			{
				spawnedZombies = 0;
				zombieSpawnRate = Mathf.Abs(zombieSpawnRate - zombieSpawnRateIncrement);
				if (zombieSpawnRate < maxSpawnRate)
				{
					zombieSpawnRate = maxSpawnRate;
				}
				zombieTimer.WaitTime = zombieSpawnRate;
				ui.UpdateSpawnRateLabel(zombieSpawnRate);
			}
		}

		Zombie zombie = (Zombie)ZombieScene.Instance();
		PathFollow2D spawnLoc = GetNode<PathFollow2D>("SpawnPath/SpawnLoc");

		spawnLoc.UnitOffset = GD.Randf();

		zombie.Position = spawnLoc.Position;
		zombie.SetPlayer(GetNode<Player>("Player"));

		zombie.Connect(nameof(Zombie.ZombieKilled), this, nameof(OnZombieKilled));

		AddChild(zombie);
	}

    private void OnZombieKilled(Vector2 position, bool dropped)
	{
		zombieDeathSounds.GetChild<AudioStreamPlayer>(Utilities.RandNum(zombieDeathSounds.GetChildCount())).Play();
		ui.UpdateScoreLabel(1);
		if (dropped)
		{
            PlaceBonus(Utilities.GetRandomSceneFromList(BonusesScenes), position);
        }
		Blood blood = (Blood)BloodScene.Instance();
		blood.Position = position;
		CallDeferred("add_child", blood);
	}

    private void PlaceBonus(PackedScene bonusScene, Vector2 position)
	{
		Bonus bonus = (Bonus)bonusScene.Instance();
        bonus.Position = position;
        bonus.Connect(nameof(Bonus.BonusPicked), this, nameof(OnBonusPicked));
        CallDeferred("add_child", bonus);
    }

	private void OnBonusPicked(Bonus bonus)
	{
        pickUpSound.Play();
		bonus.DoBonus(player);
    }

	private void FreeOneShotEmitters()
	{
		foreach (Particles2D emitter in GetTree().GetNodesInGroup("oneShotEmitters"))
			if (!emitter.Emitting) 
				emitter.QueueFree();
    }
}
