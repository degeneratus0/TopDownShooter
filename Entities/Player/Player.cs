using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Player : CharacterBody2D
{
	[Signal] public delegate void PlayerKilledEventHandler();
	[Signal] public delegate void AmmoUpdatedEventHandler(int currentAmmo, int maxAmmo);
	[Signal] public delegate void BonusPickedEventHandler(Bonus bonus, Player player);

	public int MaxHealth
	{
		get
		{
			return maxHealth;
		}
	}
	public int Damage
	{
		get
		{
			return damage;
		}
		set
		{
			damage = value;
		}
	}
	public int ClipSize 
	{ 
		get
		{
			return clipSize;
		}
	}
	public int Ammo
	{
		get
		{
			return ammo;
		}
	}
	public int Speed
	{
		set
		{
			speed = value;
		}
	}
	public bool Piercing
	{
		set
		{
			piercing = value;
		}
	}

	public Area2D Hitbox;
	public Area2D MeleeArea;

	private int maxHealth;
	private int health;
	private int speed;
	private int ammo;
	private int damage;
	private int clipSize;
	private float fireRate;
	private float spreading;
	private float reloadTime;
	private int bulletsPerShot;
	private int bulletSpeed;
	private int bulletSpeedRandomness;
	private bool invincible;

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
	private Node2D controlsContainer;
	private TextureProgressBar healthBar;
	private TextureProgressBar armorBar;
	private TextureProgressBar reloadBar;
	private GpuParticles2D punchEffect;

	private PackedScene EjectedShellScene;
	private PackedScene BarrelFireScene;
	private PackedScene ShotSoundScene;

	private Vector2 ScreenSize;
	private Vector2 velocity;

	private List<BonusBar> bonusBars = new List<BonusBar>();

	public override void _Ready()
	{
		LoadScenes();
		GetNodes();
		InitPlayer();

		ScreenSize = GetViewportRect().Size;
	}

	public void InitPlayer()
	{
		damage = GlobalSettings.Player.Damage;
		maxHealth = GlobalSettings.Player.MaxHP;
		speed = GlobalSettings.Player.Speed;
		fireRate = 60 / (float)GlobalSettings.Player.FireRate;
		clipSize = GlobalSettings.Player.ClipSize;
		ammo = GlobalSettings.Player.Ammo;
		reloadTime = GlobalSettings.Player.ReloadTime;
		spreading = Mathf.DegToRad(GlobalSettings.Player.Spreading);
		bulletsPerShot = GlobalSettings.Player.BulletsPerShot;
		bulletSpeed = GlobalSettings.Player.BulletSpeed;
		bulletSpeedRandomness = GlobalSettings.Player.BulletSpeedRandomness;
		invincible = GlobalSettings.Player.IsInvincible;

		InitUi();

		fromReload = 0;

		UpdateAmmo(0);
	}

	private void InitUi()
	{
		healthBar.MaxValue = maxHealth;
		healthBar.Value = maxHealth;
		health = maxHealth;

		reloadBar.MaxValue = reloadTime;
		reloadBar.Value = reloadTime;
	}

	private void LoadScenes()
	{
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
			velocity = Input.GetVector(Controls.Left, Controls.Right, Controls.Up, Controls.Down);

			Velocity = velocity.Normalized() * speed;
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

		if (armor > 0 && value < 0)
		{
			armor += value;
			armorBar.Value = armor;
			return;
		}
		if (health + value > maxHealth)
		{
			health = maxHealth;
		}
		else
		{
			health += value;
		}

		healthBar.Value = health;

		if (health <= 0)
		{
			Kill();
		}
	}

	public void AddArmor()
	{
		armor = 100;
		armorBar.Value = armor;
	}

	public void InvokeTimedBonus(Bonus bonus, int seconds, Action callback)
	{
		EmitSignal(SignalName.BonusPicked, bonus, this);
		if (!bonusBars.Any(x => x.Bonus == bonus))
		{
			BonusBar bonusBar = new BonusBar(bonus, seconds, callback);
			bonusBars.Add(bonusBar);
			controlsContainer.AddChild(bonusBar);
			controlsContainer.MoveChild(bonusBar, 0);
		}
		else
		{
			BonusBar bar = bonusBars.Single(x => x.Bonus == bonus);
			bar.Reset();
			controlsContainer.MoveChild(bar, 0);
		}
	}

	public void UpdateAmmo(int ammoChange)
	{
		ammo += ammoChange;
		EmitSignal(SignalName.AmmoUpdated, clipSize, ammo);
	}

	private void Melee()
	{
		if (!punching && Input.IsActionJustPressed(Controls.Melee) && fromMelee >= meleeCooldown)
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
			if (Input.IsActionPressed(Controls.Shoot) && fromShot >= fireRate && clipSize > 0 && !reloading && !reloadFinishSound.Playing)
			{
				clipSize--;
				fromShot = 0;
				if (bulletsPerShot > 1)
				{
					for (int i = 0; i < bulletsPerShot; i++)
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
			if (Input.IsActionJustPressed(Controls.Shoot) && clipSize == 0)
			{
				emptyClipSound.Play();
			}
			if (Input.IsActionJustPressed(Controls.Reload))
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
			reloadBar.Value = reloadTime - fromReload;
			if (fromReload >= reloadTime)
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
		var shotSound = (AudioStreamPlayer2D)ShotSoundScene.Instantiate();
		shotSound.Finished += shotSound.QueueFree;
		if (clipSize <= 5)
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
		Bullet bullet = ScenePool<Bullet>.Take();
		bullet.Transform = gunTip.GlobalTransform;
		bullet.Rotate((float)GD.RandRange(-spreading, spreading));
		
		bullet.Shot(bulletSpeed, bulletSpeedRandomness, piercing);
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
		var barrelFire = (GpuParticles2D)BarrelFireScene.Instantiate();
		barrelFire.Transform = gunTip.GlobalTransform;
		barrelFire.Finished += barrelFire.QueueFree;

		GetParent().AddChild(barrelFire);

		barrelFire.Emitting = true;
		barrelFire.GetNode<GpuParticles2D>("AdditionalFire").Emitting = true;
	}

	private void SpawnPunchEffect()
	{
		punchEffect.Emitting = true;
	}

	private void Reload()
	{
		if (ammo > 0 && !reloading)
		{
			reloadStartSound.Play();
			reloading = true;
			clipSize = GlobalSettings.Player.ClipSize;
			UpdateAmmo(-clipSize);
			reloadBar.Show();
		}
	}

	private void OnAnimationFinished(StringName animation)
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
		if (Input.IsActionJustPressed(Controls.LaserSwitch))
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
