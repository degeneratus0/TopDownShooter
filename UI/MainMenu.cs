using Godot;
using System;

public class MainMenu : Control
{
	private Control StartScreen;
	private Control OptionsScreen;

	private SpinBox ammoSpinBox;

	const string DifficultyOptionsPath = "OptionsScreen/DifficultyOptionsPanel/";
	const string PlayerOptionsPath = "OptionsScreen/PlayerOptionsPanel/";

	public override void _Ready()
	{
		StartScreen = GetNode<Control>("StartScreen");
		OptionsScreen = GetNode<Control>("OptionsScreen");
		ammoSpinBox = GetNode<SpinBox>(PlayerOptionsPath + "AmmoSpinBox");
	}

	private void InitFields()
	{
		GetNode<SpinBox>(DifficultyOptionsPath + "ZombieSpawnRateSpinBox").Value = GlobalSettings.Difficulty.ZombieSpawnRate;
		GetNode<SpinBox>(DifficultyOptionsPath + "SpawnRateIncrementSpinBox").Value = GlobalSettings.Difficulty.ZombieSpawnRateIncrement;
		GetNode<SpinBox>(DifficultyOptionsPath + "ZombiesToIncSpinBox").Value = GlobalSettings.Difficulty.ZombiesToIncrement;
		GetNode<SpinBox>(DifficultyOptionsPath + "MaxSpawnRateSpinBox").Value = GlobalSettings.Difficulty.MaxSpawnRate;
		GetNode<CheckBox>(DifficultyOptionsPath + "ShowSpawnRateCheckBox").Pressed = GlobalSettings.Difficulty.ShowSpawnRate;
		GetNode<SpinBox>(DifficultyOptionsPath + "ZombieMinSpeedSpinBox").Value = GlobalSettings.Zombie.ZombieMinSpeed;
		GetNode<SpinBox>(DifficultyOptionsPath + "ZombieMaxSpeedSpinBox").Value = GlobalSettings.Zombie.ZombieMaxSpeed;
		GetNode<SpinBox>(DifficultyOptionsPath + "DropChanceSpinBox").Value = GlobalSettings.Zombie.DropChance;
		
		GetNode<SpinBox>(PlayerOptionsPath + "SpeedSpinBox").Value = GlobalSettings.Player.Speed;
		GetNode<SpinBox>(PlayerOptionsPath + "FireRateSpinBox").Value = GlobalSettings.Player.FireRate;
		GetNode<SpinBox>(PlayerOptionsPath + "ClipSizeSpinBox").Value = GlobalSettings.Player.ClipSize;

		GetNode<SpinBox>(PlayerOptionsPath + "ReloadTimeSpinBox").Value = GlobalSettings.Player.ReloadTime;
		GetNode<SpinBox>(PlayerOptionsPath + "SpreadingSpinBox").Value = GlobalSettings.Player.Spreading;
		GetNode<SpinBox>(PlayerOptionsPath + "BulletsPerShotSpinBox").Value = GlobalSettings.Player.BulletsPerShot;
		GetNode<SpinBox>(PlayerOptionsPath + "BulletSpeedSpinBox").Value = GlobalSettings.Player.BulletSpeed;
		GetNode<SpinBox>(PlayerOptionsPath + "BulSpeedRandSpinBox").Value = GlobalSettings.Player.BulletSpeedRandomness;
		GetNode<CheckBox>(PlayerOptionsPath + "InvincibleCheckBox").Pressed = GlobalSettings.Player.IsInvincible;

		ammoSpinBox.Value = GlobalSettings.Player.Ammo;
		ammoSpinBox.Step = GlobalSettings.Player.ClipSize;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("toggle_fullscreen"))
		{
			OS.WindowFullscreen = !OS.WindowFullscreen;
		}
	}

	public void OnStartButtonPressed()
	{
		GetTree().ChangeScene("res://World.tscn");
	}

	public void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}

	public void OnOptionsButtonPressed()
	{
		InitFields();
		StartScreen.Visible = false;
		OptionsScreen.Visible = true;
	}

	public void OnBackButtonPressed()
	{
		StartScreen.Visible = true;
		OptionsScreen.Visible = false;
	}

	public void OnZombieSpawnRateChanged(float value) => GlobalSettings.Difficulty.ZombieSpawnRate = value;

	public void OnSpawnRateIncrementChanged(float value) => GlobalSettings.Difficulty.ZombieSpawnRateIncrement = (int)value;

	public void OnZombiesToIncChanged(float value) => GlobalSettings.Difficulty.ZombiesToIncrement = (int)value;

	public void OnMaxSpawnRateChanged(float value) => GlobalSettings.Difficulty.MaxSpawnRate = value;

	public void OnShowSpawnRateToggled(bool value) => GlobalSettings.Difficulty.ShowSpawnRate = value;

	public void OnZombieMinSpeedChanged(float value) => GlobalSettings.Zombie.ZombieMinSpeed = (int)value;

	public void OnZombieMaxSpeedChanged(float value) => GlobalSettings.Zombie.ZombieMaxSpeed = (int)value;

	public void OnDropChanceChanged(float value) => GlobalSettings.Zombie.DropChance = (int)value;

	public void OnResetDifficultyDefaultButtonPressed()
	{
		GlobalSettings.Difficulty.SetDefault();
		InitFields();
	}

	public void OnSpeedChanged(float value) => GlobalSettings.Player.Speed = (int)value;

	public void OnFireRateChanged(float value) => GlobalSettings.Player.FireRate = (int)value;

	public void OnClipSizeChanged(float value)
	{
		GlobalSettings.Player.ClipSize = (int)value;
		if (value == 0)
		{
			ammoSpinBox.Step = 1;
		}
		else
		{
			ammoSpinBox.Step = (int)value;
		}
		ammoSpinBox.Value = ammoSpinBox.Value;
		GlobalSettings.Player.Ammo = (int)ammoSpinBox.Value;
	}
	public void OnAmmoChanged(float value) => GlobalSettings.Player.Ammo = (int)value;

	public void OnReloadTimeChanged(float value) => GlobalSettings.Player.ReloadTime = value;
	
	public void OnSpreadingChanged(float value) => GlobalSettings.Player.Spreading = (int)value;

	public void OnBulletsPerShotChanged(float value) => GlobalSettings.Player.BulletsPerShot = (int)value;

	public void OnBulletSpeedChanged(float value) => GlobalSettings.Player.BulletSpeed = (int)value;

	public void OnBulSpeedRandChanged(float value) => GlobalSettings.Player.BulletSpeedRandomness = (int)value;

	public void OnInvincibleToggled(bool value) => GlobalSettings.Player.IsInvincible = value;

	public void OnResetPlayerDefaultButtonPressed()
	{
		GlobalSettings.Player.SetDefault();
		InitFields();
	}
}
