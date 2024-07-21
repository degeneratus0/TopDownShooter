using Godot;
using Shootem.Entities;

public partial class World : Node2D
{
	private Player player;
	private UI ui;
	private Control deathScreen;
	private Control pauseScreen;
	private Control winScreen;
	private AudioStreamPlayer playerDeathSound;
	private AudioStreamPlayer pickUpSound;

	private Level currentLevel;
	private MobManager mobManager;

	private static StringName uiPath = "UI/Control/";

	public override void _Ready()
	{
		PackedScene levelScene = GD.Load<PackedScene>("res://Levels/Arena.tscn");
		Level level = (Level)levelScene.Instantiate();
		AddChild(level);
		currentLevel = level;

		GetNodes();

		player.PlayerKilled += GameOver;
		player.AmmoUpdated += ui.UpdateAmmoLabel;

		ui.UpdateAmmoLabel(player.ClipSize, player.Ammo);
	}

	private void GetNodes()
	{
		player = GetNode<Player>("Arena/Player");
		ui = GetNode<UI>("UI");
		deathScreen = GetNode<Control>(uiPath + "DeathScreen");
		pauseScreen = GetNode<Control>(uiPath + "PauseScreen");
		winScreen = GetNode<Control>(uiPath + "WinScreen");
		playerDeathSound = GetNode<AudioStreamPlayer>("Sounds/PlayerDeathSound");
		pickUpSound = GetNode<AudioStreamPlayer>("Sounds/PickUpSound");

		mobManager = GetNode<MobManager>("MobManager");
		mobManager.Init(player, currentLevel, ui);
	}

	public void NewGame()
	{
		GetTree().ReloadCurrentScene();
	}

	public void GameOver()
	{
		playerDeathSound.Play();
		deathScreen.Show();
	}

	private void OnEndTriggerBodyEntered(Node2D	body)
	{
		if (body is not Player)
		{
			return;
		}
		winScreen.Show();
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
		if (@event.IsActionPressed(Controls.Reload) && (deathScreen.Visible || winScreen.Visible))
		{
			NewGame();
		}
		if (@event.IsActionPressed(Controls.Quit))
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
		if (@event.IsActionPressed(Controls.Menu) && pauseScreen.Visible)
		{
			GetTree().Paused = false;
			GetTree().ChangeSceneToFile("res://UI/MainMenu.tscn");
		}
		if (@event.IsActionPressed(Controls.ToggleFullscreen))
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
			player.InitPlayer(); //TODO: Init weapon
		}
	}
}
