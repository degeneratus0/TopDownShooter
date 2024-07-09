using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Signal] public delegate void PlayerKilledEventHandler();
	[Signal] public delegate void AmmoUpdatedEventHandler(int currentAmmo, int maxAmmo);
	[Signal] public delegate void BonusPickedEventHandler(BonusType bonus, Player player);

	public int Damage { 
		get
		{
			return currentDamage;
		}
		set
		{
			currentDamage = value;
		}
	}
	public int MaxHP;
	public int Speed;
	public float FireRate { 
		get 
		{ 
			return fireRate;
		} 
		set
		{
			fireRate = 60 / value;
		} 
	}
	public int ClipSize 
	{ 
		get
		{
			return currentClip;
		}
		set
		{
			currentClip = value;
		}
	}
	public int Ammo;
	public float ReloadTime;
	public int BulletsPerShot;
	public int BulletSpeed;
	public int BulletSpeedRandomness;
	public int HP;

	public Area2D Hitbox;
	public Area2D MeleeArea;

	private bool invincible;
	private int currentDamage;
	private float spreading;
	private float fireRate;
	private int currentClip;
	private readonly float meleeCooldown = 0.5f;
	private double fromMelee = 0;
	private double fromShot = 0;
	private double fromReload = 0;
	private bool reloading = false;
	private bool laserOn = false;
	private bool isDead = false;
	private bool punching = false;
	private bool shooting = false;
	private bool piercing = false;
	private int armor = 0;

	private AudioStreamPlayer2D reloadStartSound;
	private AudioStreamPlayer2D reloadFinishSound;
	private AudioStreamPlayer2D emptyClipSound;
	private AnimatedSprite2D walkingSprite;
	private AnimatedSprite2D shootingSprite;
	private AnimationPlayer animationPlayer;
	private Line2D line;
	private Marker2D gunTip;
	private Marker2D bulletEjector;
	private Marker2D meleeHitPoint;
	private CollisionShape2D meleeCollision;
	private Node2D controlsContainer;
	private TextureProgressBar healthBar;
	private TextureProgressBar armorBar;
	private TextureProgressBar reloadBar;
	private GpuParticles2D punchEffect;

	private PackedScene BulletScene;
	private PackedScene EjectedShellScene;
	private PackedScene BarrelFireScene;
	private PackedScene ShotSoundScene;

	private Vector2 ScreenSize;
	private Vector2 velocity;

	public override void _Ready()
	{
		LoadScenes();
		GetNodes();
		InitPlayer();

		ScreenSize = GetViewportRect().Size;
	}

	public void InitPlayer()
	{
		Damage = GlobalSettings.Player.Damage;
		MaxHP = GlobalSettings.Player.MaxHP;
		Speed = GlobalSettings.Player.Speed;
		FireRate = GlobalSettings.Player.FireRate;
		ClipSize = GlobalSettings.Player.ClipSize;
		Ammo = GlobalSettings.Player.Ammo;
		ReloadTime = GlobalSettings.Player.ReloadTime;
		spreading = Mathf.DegToRad(GlobalSettings.Player.Spreading);
		BulletsPerShot = GlobalSettings.Player.BulletsPerShot;
		BulletSpeed = GlobalSettings.Player.BulletSpeed;
		BulletSpeedRandomness = GlobalSettings.Player.BulletSpeedRandomness;
		invincible = GlobalSettings.Player.IsInvincible;

		InitUi();

		fromReload = 0;

		UpdateAmmo(0);
	}

	private void InitUi()
	{
		healthBar.MaxValue = MaxHP;
		healthBar.Value = MaxHP;
		HP = MaxHP;

		reloadBar.MaxValue = ReloadTime;
		reloadBar.Value = ReloadTime;
	}

	private void LoadScenes()
	{
		BulletScene = GD.Load<PackedScene>("res://Entities/Player/PlayerObjects/Bullet.tscn");
		EjectedShellScene = GD.Load<PackedScene>("res://Entities/Player/PlayerObjects/EjectedShell.tscn");
		BarrelFireScene = GD.Load<PackedScene>("res://Entities/Player/PlayerObjects/BarrelFire.tscn");
		ShotSoundScene = GD.Load<PackedScene>("res://Entities/Player/PlayerObjects/ShotSound.tscn");
	}

	private void GetNodes()
	{
		Hitbox = GetNode<Area2D>("Hitbox");
		MeleeArea = GetNode<Area2D>("MeleeArea");
		reloadStartSound = GetNode<AudioStreamPlayer2D>("Sounds/ReloadStartSound");
		reloadFinishSound = GetNode<AudioStreamPlayer2D>("Sounds/ReloadFinishSound");
		emptyClipSound = GetNode<AudioStreamPlayer2D>("Sounds/EmptyClipSound");
		walkingSprite = GetNode<AnimatedSprite2D>("WalkingSprite");
		shootingSprite = GetNode<AnimatedSprite2D>("ShootingSprite");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		line = GetNode<Line2D>("Line2D");
		gunTip = GetNode<Marker2D>("GunTip");
		bulletEjector = GetNode<Marker2D>("BulletEjector");
		meleeHitPoint = GetNode<Marker2D>("MeleeHitPoint");
		meleeCollision = GetNode<CollisionShape2D>("MeleeArea/MeleeCollision");
		controlsContainer = GetNode<Node2D>("ControlsContainer");
		healthBar = GetNode<TextureProgressBar>("ControlsContainer/HealthBar");
		armorBar = GetNode<TextureProgressBar>("ControlsContainer/ArmorBar");
		reloadBar = GetNode<TextureProgressBar>("ControlsContainer/ReloadBar");
		punchEffect = GetNode<GpuParticles2D>("MeleeHitPoint/PunchEffect");
	}

	public override void _Process(double delta)
	{
		controlsContainer.GlobalRotation = 0;
		CountCooldowns(delta);
		SetWalkingAnimation();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!isDead)
		{
			velocity = new Vector2(                
				Input.GetActionStrength("right") - Input.GetActionStrength("left"),
				Input.GetActionStrength("down") - Input.GetActionStrength("up"));

			Velocity = velocity.Normalized() * Speed;
			MoveAndSlide();

			LookAt(GetGlobalMousePosition());

			Melee();
			Shooting();
			LaserControl();
		}
	}

	public void Kill()
	{
		isDead = true;
		EmitSignal(SignalName.PlayerKilled);
		Hide();
		GetNode<CollisionShape2D>("Hitbox/HitboxCollision").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
	}

	public void ChangeHP(int value)
	{
		if (invincible) return;

		if (armor > 0)
		{
			armor += value;
			armorBar.Value = armor;
			return;
		}
		if (HP + value > MaxHP)
		{
			HP = MaxHP;
		}
		else
		{
			HP += value;
		}

		healthBar.Value = HP;

		if (HP <= 0)
		{
			Kill();
		}
	}

	public void AddArmor()
	{
		armor = 100;
		armorBar.Value = armor;
	}

	public void InvokeTimedBonus(BonusType bonusType, int seconds, Color color, Action callback)
	{
		EmitSignal(SignalName.BonusPicked, (int)bonusType, this);
		BonusBar bonusBar = new BonusBar(bonusType, seconds, color, callback);
		BonusPicked += bonusBar.IsNewPicked;
		controlsContainer.AddChild(bonusBar);
		controlsContainer.MoveChild(bonusBar, 0);
	}

	public void DamageUp(int seconds, int multiplier)
	{
		InvokeTimedBonus(BonusType.DamageUp, seconds, Colors.DarkRed, DamageDown);
		currentDamage = GlobalSettings.Player.Damage * multiplier;
	}

	public void DamageDown()
	{
		currentDamage = GlobalSettings.Player.Damage;
	}

	public void SpeedUp(int seconds, double multiplier)
	{
		InvokeTimedBonus(BonusType.SpeedUp, seconds, Colors.Yellow, SpeedDown);
		Speed = (int)(GlobalSettings.Player.Speed * multiplier);
	}

	public void SpeedDown()
	{
		Speed = GlobalSettings.Player.Speed;
	}

	public void PierceUp(int seconds)
	{
		InvokeTimedBonus(BonusType.Piercing, seconds, Colors.GreenYellow, PierceDown);
		piercing = true;
	}

	public void PierceDown()
	{
		piercing = false;
	}

	public void UpdateAmmo(int ammoChange)
	{
		Ammo += ammoChange;
		EmitSignal(SignalName.AmmoUpdated, currentClip, Ammo);
	}

	private void Melee()
	{
		if (!punching && Input.IsActionJustPressed("melee") && fromMelee >= meleeCooldown)
		{
			fromMelee = 0;
			punching = true;
			animationPlayer.Play("punch");
		}
	}

	private void Shooting()
	{
		if (!punching)
		{
			if (Input.IsActionPressed("shoot") && fromShot >= fireRate && currentClip > 0 && !reloading && !reloadFinishSound.Playing)
			{
				currentClip--;
				fromShot = 0;
				if (BulletsPerShot > 1)
				{
					for (int i = 0; i < BulletsPerShot; i++)
					{
						BulletShot();
					}
				}
				else
				{
					BulletShot();
				}
				UpdateAmmo(0);
				SpawnShotSound();
				SpawnEjectedShell();
				SpawnBarrelFire();
				shooting = true;
				shootingSprite.Frame = 0;
				shootingSprite.Play("shooting");
			}
			if (Input.IsActionJustPressed("shoot") && currentClip == 0)
			{
				emptyClipSound.Play();
			}
			if (Input.IsActionJustPressed("reload"))
			{
				Reload();
			}
		}
	}

	private void CountCooldowns(double delta)
	{
		if (fromMelee < meleeCooldown)
		{
			fromMelee += delta;
		}

		if (fromShot < fireRate)
		{
			fromShot += delta;
		}
		else
		{
			shooting = false;
		}

		if (reloading)
		{
			fromReload += delta;
			reloadBar.Value = ReloadTime - fromReload;
			if (fromReload >= ReloadTime)
			{
				reloadFinishSound.Play();
				reloading = false;
				fromReload = 0;
				reloadBar.Hide();
			}
		}
	}

	private void SpawnShotSound()
	{
		ShotSound shotSound = (ShotSound)ShotSoundScene.Instantiate();
		if (currentClip <= 5)
		{
			shotSound.PitchScale = 1.5f;
		}
		else
		{
			shotSound.PitchScale = 1;
		}
		AddChild(shotSound);
	}

	private void BulletShot()
	{
		Bullet bullet = (Bullet)BulletScene.Instantiate();
		bullet.Transform = gunTip.GlobalTransform;
		bullet.Rotate((float)GD.RandRange(-spreading, spreading));
		
		bullet.Shot(BulletSpeed, BulletSpeedRandomness, piercing);
		GetParent().AddChild(bullet);
	}

	private void SpawnEjectedShell()
	{
		EjectedShell ejectedShell = (EjectedShell)EjectedShellScene.Instantiate();
		ejectedShell.Transform = bulletEjector.GlobalTransform;

		ejectedShell.Eject(velocity);
		GetParent().AddChild(ejectedShell);
	}

	private void SpawnBarrelFire()
	{
		BarrelFire barrelFire = (BarrelFire)BarrelFireScene.Instantiate();
		barrelFire.Transform = gunTip.GlobalTransform;
		GetParent().AddChild(barrelFire);
	}

	private void SpawnPunchEffect()
	{
		punchEffect.Emitting = true;
	}

	private void Reload()
	{
		if (Ammo > 0 && !reloading)
		{
			reloadStartSound.Play();
			reloading = true;
			currentClip = GlobalSettings.Player.ClipSize;
			UpdateAmmo(-currentClip);
			reloadBar.Show();
		}
	}

	private void OnAnimationFinished(string animation)
	{
		if (animation == "punch")
		{
			punching = false;
			animationPlayer.Play("idle");
		}
		else if (animation == "shooting")
		{
			shooting = false;
			//animationPlayer.Play("idle");
		}
	}

	private void SetWalkingAnimation()
	{
		walkingSprite.GlobalRotationDegrees = Mathf.RadToDeg(velocity.Angle()) + 90;
		if (velocity != Vector2.Zero)
		{
			walkingSprite.Animation = "walking";
		}
		else
		{
			walkingSprite.Animation = "idle";
		}
	}

	private void LaserControl()
	{
		if (Input.IsActionJustPressed("laserSwitch"))
		{
			laserOn = !laserOn;
		}
		line.Points = null;
		if (laserOn)
		{
			spreading = Mathf.DegToRad(GlobalSettings.Player.Spreading) * 0.5f;
			line.AddPoint(Vector2.Zero);
			line.AddPoint(GetLocalMousePosition().Normalized() * 500);
		}
		else
		{
			spreading = Mathf.DegToRad(GlobalSettings.Player.Spreading);
		}
	}
}
