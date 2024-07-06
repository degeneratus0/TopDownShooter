using Godot;
using Shootem.Entities;
using System.Collections.Generic;
using System.Linq;

public partial class World : Node2D
{
	private int ZombiesToIncrement;

	private float zombieSpawnRate;
	private float zombieSpawnRateIncrement;
	private float maxSpawnRate;

	private int liveZombiesCount = 0;
	private int spawnedZombies = 0;
	private int spawnsCount;

    private double soundTime = 0;
    private double minSoundTime = 5;
    private double maxSoundTime = 20;

    private Player player;
	private Timer zombieTimer;
	private UI ui;
	private Control deathScreen;
	private Control pauseScreen;
    private Control winScreen;
	private AudioStreamPlayer playerDeathSound;
	private AudioStreamPlayer pickUpSound;
    private Node zombieDeathSounds;
    private Node zombieAmbientSounds;
    private Node spawnsFolder;

    private PackedScene ZombieScene;
	private PackedScene BloodScene;
	private List<PackedScene> BonusesScenes;

	private const string uiPath = "UI/Control/";

	public override void _Ready()
	{
		GD.Randomize();

		GetNodes();
		LoadScenes();
		InitWorld();

		player.PlayerKilled += GameOver;
		player.AmmoUpdated += ui.UpdateAmmoLabel;

		zombieTimer.WaitTime = zombieSpawnRate;
		ui.UpdateSpawnRateLabel(zombieSpawnRate);
		ui.UpdateAmmoLabel(player.ClipSize, player.Ammo);

        soundTime = GD.RandRange(minSoundTime, maxSoundTime);
    }

	private void GetNodes()
	{
		player = GetNode<Player>("Player");
		zombieTimer = GetNode<Timer>("ZombieTimer");
		ui = GetNode<UI>("UI");
		deathScreen = GetNode<Control>(uiPath + "DeathScreen");
		pauseScreen = GetNode<Control>(uiPath + "PauseScreen");
        winScreen = GetNode<Control>(uiPath + "WinScreen");
		playerDeathSound = GetNode<AudioStreamPlayer>("Sounds/PlayerDeathSound");
		pickUpSound = GetNode<AudioStreamPlayer>("Sounds/PickUpSound");
        zombieDeathSounds = GetNode<Node>("Sounds/ZombieDeathSounds");
        zombieAmbientSounds = GetNode<Node>("Sounds/ZombieAmbientSounds");
        spawnsFolder = GetNode<Node>("Spawns");
    }

	private void LoadScenes()
	{
		ZombieScene = GD.Load<PackedScene>("res://Entities/Zombie/Zombie.tscn");
		BloodScene = GD.Load<PackedScene>("res://Entities/Zombie/ZombieObjects/Blood.tscn");

		BonusesScenes = new List<PackedScene>()
		{
			GD.Load<PackedScene>("res://Entities/Bonuses/BulletPack.tscn"),
			GD.Load<PackedScene>("res://Entities/Bonuses/Aid.tscn"),
			GD.Load<PackedScene>("res://Entities/Bonuses/DamageUp.tscn")
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

	public override void _PhysicsProcess(double delta)
	{
		if (!GetTree().Paused)
        {
            soundTime -= delta;
        }

        if (soundTime <= 0 && liveZombiesCount > 0)
        {
			minSoundTime = 500 / liveZombiesCount;
            maxSoundTime = 1000 / liveZombiesCount;
            soundTime = GD.RandRange(minSoundTime, maxSoundTime);
            zombieAmbientSounds.GetChild<AudioStreamPlayer>(Utilities.RandNum(zombieAmbientSounds.GetChildCount())).Play();
        }
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

	private void OnEndTriggerBodyEntered(Node2D	body)
	{
		if (body is not Player)
		{
			return;
		}
        winScreen.Show();
        zombieTimer.Stop();
		foreach (Node zombie in GetTree().GetNodesInGroup("zombies"))
		{
			if (zombie is Zombie)
			{
				((Zombie)zombie).Kill();
			}
		}
	}


    public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("reload") && (deathScreen.Visible || winScreen.Visible))
		{
			NewGame();
		}
		if (@event.IsActionPressed("quit"))
		{
			if (deathScreen.Visible || winScreen.Visible)
			{
				GetTree().ChangeSceneToFile("res://UI/MainMenu.tscn");
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
			GetTree().ChangeSceneToFile("res://UI/MainMenu.tscn");
		}
		if (@event.IsActionPressed("toggle_fullscreen"))
		{
			if (DisplayServer.WindowGetMode() != DisplayServer.WindowMode.Fullscreen)
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            }
			else
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
            }
		}
	}

	public void OnZombieTimerTimeout()
    {
        FreeOneShotEmitters(); //to not call it in physics process every frame

        if (liveZombiesCount >= 200)
		{
			return;
		}
        List<Spawner> spawners = spawnsFolder.GetChildren()
			.Select(x => (Spawner)x)
			.Where(x => !x.VisibilityNotifier.IsOnScreen() && x.GlobalPosition.DistanceTo(player.GlobalPosition) < 1500)
			.ToList();
		if (!spawners.Any())
        {
            zombieTimer.WaitTime = zombieSpawnRate;
            return;
		}

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

        ui.UpdateZombikov(++liveZombiesCount);

        Zombie zombie = (Zombie)ZombieScene.Instantiate();
        zombie.Position = spawners[GD.RandRange(0, spawners.Count - 1)].Position;
		zombie.SetPlayer(GetNode<Player>("Player"));

		zombie.ZombieKilled += OnZombieKilled;

		AddChild(zombie);
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
			PlaceBonus(Utilities.GetRandomSceneFromList(BonusesScenes), position);
		}
		Blood blood = (Blood)BloodScene.Instantiate();
		blood.Position = position;
		CallDeferred(Node.MethodName.AddChild, blood);
	}

	private void PlaceBonus(PackedScene bonusScene, Vector2 position)
	{
		Bonus bonus = (Bonus)bonusScene.Instantiate();
		bonus.Position = position;
		CallDeferred(Node.MethodName.AddChild, bonus);
	}

	public void OnPicked(Pickable pickable)
	{
		pickUpSound.Play();
		if (pickable is Bonus bonus)
        {
            bonus.DoBonus(player);
        }
		else if (pickable is GenericWeapon weapon)
		{
			ui.UpdateWeaponTexture(weapon.Texture);
            weapon.UpdateConfiguration();
            player.InitPlayer();
        }
	}

	private void FreeOneShotEmitters()
	{
		foreach (GpuParticles2D emitter in GetTree().GetNodesInGroup("oneShotEmitters"))
			if (!emitter.Emitting) 
				emitter.QueueFree();
	}
}
