using Godot;
using System;

public class Player : KinematicBody2D
{
	[Signal] public delegate void PlayerKilled();
	[Signal] public delegate void AmmoUpdated(int currentAmmo, int maxAmmo);

    public int Damage;
    public int MaxHP;
    public int Speed;
	public int FireRate;
	public int ClipSize;
	public int Ammo;
	public float ReloadTime;
	public float Spreading;
	public int BulletsPerShot;
	public int BulletSpeed;
	public int BulletSpeedRandomness;
	public bool IsInvincible;

    public int currentDamage;
    public int HP;
    private float currentSpreading;
	private float fireRate;
	private int currentClip;
	private readonly float meleeCooldown = 0.5f;
	private float fromMelee = 0;
	private float fromShot = 0;
	private float fromReload = 0;
	private bool reloading = false;
	private bool laserOn = false;
	private bool isDead = false;
	private bool punching = false;
	private bool shooting = false;

	private AudioStreamPlayer2D reloadStartSound;
	private AudioStreamPlayer2D reloadFinishSound;
	private AudioStreamPlayer2D emptyClipSound;
	private AnimatedSprite walkingSprite;
	private AnimatedSprite shootingSprite;
	private Line2D line;
	private Position2D gunTip;
    private Position2D bulletEjector;
    private Position2D meleeHitPoint;
    private RayCast2D meleeRayCast;
    private TextureProgress hpBar;
    private Node2D controlsContainer;
    private Timer damageUpTimer;
	private Particles2D punchEffect;

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

        Connect(nameof(PlayerKilled), GetParent(), nameof(World.GameOver));

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
        Spreading = Mathf.Deg2Rad(GlobalSettings.Player.Spreading);
        BulletsPerShot = GlobalSettings.Player.BulletsPerShot;
		BulletSpeed = GlobalSettings.Player.BulletSpeed;
		BulletSpeedRandomness = GlobalSettings.Player.BulletSpeedRandomness;
		IsInvincible = GlobalSettings.Player.IsInvincible;

        currentDamage = Damage;
        currentClip = ClipSize;
        fireRate = 60 / (float)FireRate;
        currentSpreading = Spreading;

        hpBar.MaxValue = MaxHP;
        hpBar.Value = MaxHP;
        HP = MaxHP;

        fromReload = 0;

        UpdateAmmo(0);
    }

	private void LoadScenes()
	{
        BulletScene = GD.Load<PackedScene>("res://Objects/PlayerObjects/Bullet.tscn");
        EjectedShellScene = GD.Load<PackedScene>("res://Objects/PlayerObjects/EjectedShell.tscn");
        BarrelFireScene = GD.Load<PackedScene>("res://Objects/PlayerObjects/BarrelFire.tscn");
        ShotSoundScene = GD.Load<PackedScene>("res://Objects/PlayerObjects/ShotSound.tscn");
    }

	private void GetNodes()
	{
        reloadStartSound = GetNode<AudioStreamPlayer2D>("Sounds/ReloadStartSound");
        reloadFinishSound = GetNode<AudioStreamPlayer2D>("Sounds/ReloadFinishSound");
        emptyClipSound = GetNode<AudioStreamPlayer2D>("Sounds/EmptyClipSound");
        walkingSprite = GetNode<AnimatedSprite>("WalkingSprite");
        shootingSprite = GetNode<AnimatedSprite>("ShootingSprite");
        line = GetNode<Line2D>("Line2D");
        gunTip = GetNode<Position2D>("GunTip");
        bulletEjector = GetNode<Position2D>("BulletEjector");
        meleeHitPoint = GetNode<Position2D>("MeleeHitPoint");
        meleeRayCast = GetNode<RayCast2D>("MeleeRayCast");
        hpBar = GetNode<TextureProgress>("ControlsContainer/HPBar");
        controlsContainer = GetNode<Node2D>("ControlsContainer");
        damageUpTimer = GetNode<Timer>("DamageUpTimer");
        punchEffect = GetNode<Particles2D>("MeleeHitPoint/PunchEffect");
    }

    public override void _Process(float delta)
    {
        controlsContainer.GlobalRotation = 0;
        CountCooldowns(delta);
        SetAnimation();
    }

    public override void _PhysicsProcess(float delta)
	{
        if (!isDead)
		{
			velocity = new Vector2();
			if (Input.IsActionPressed("up"))
			{
				velocity.y -= 1;
			}
			if (Input.IsActionPressed("down"))
			{
				velocity.y += 1;
			}
			if (Input.IsActionPressed("right"))
			{
				velocity.x += 1;
			}
			if (Input.IsActionPressed("left"))
			{
				velocity.x -= 1;
			}

			MoveAndCollide(velocity.Normalized() * Speed * delta);
			Position = new Vector2(
			Mathf.Clamp(Position.x, 0, ScreenSize.x),
			Mathf.Clamp(Position.y, 0, ScreenSize.y)
			);
			LookAt(GetGlobalMousePosition());

            Melee();
            Shooting();
			LaserControl();
        }
    }

    public void Kill()
    {
        if (!IsInvincible)
        {
            isDead = true;
            EmitSignal("PlayerKilled");
            Hide();
            GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
        }
    }

    public void ChangeHP(int value)
	{
		if (HP + value > MaxHP)
		{
            HP = MaxHP;
		}
		else
        {
            HP += value;
        }

		hpBar.Value = HP;

		if (HP <= 0)
		{
			Kill();
		}
	}

	public void DamageUp(int multiplier)
	{
		currentDamage = Damage * multiplier;
        damageUpTimer.Start();
	}

	public void OnDamageUpTimerTimeout()
	{
		currentDamage = Damage;
    }

    public void UpdateAmmo(int ammoChange)
    {
        Ammo += ammoChange;
        EmitSignal(nameof(AmmoUpdated), currentClip, Ammo);
    }

    private void Melee()
	{
        if (!punching && Input.IsActionJustPressed("melee") && fromMelee >= meleeCooldown)
        {
            fromMelee = 0;
			punching = true;
        }
        if (punching && shootingSprite.Frame == 2 && meleeRayCast.IsColliding() && meleeRayCast.GetCollider() is Zombie zombie)
            {
                zombie.Kill();
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
                        BulletShot(currentDamage);
                    }
                }
                else
                {
                    BulletShot(currentDamage);
                }
                UpdateAmmo(0);
                SpawnShotSound();
                SpawnEjectedShell();
                SpawnBarrelFire();
                shooting = true;
                shootingSprite.Frame = 0;
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

    private void CountCooldowns(float delta)
    {
        if (fromMelee < meleeCooldown)
        {
            fromMelee += delta;
        }
        else
        {
            punching = false;
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
            if (fromReload >= ReloadTime)
            {
                reloadFinishSound.Play();
                reloading = false;
                fromReload = 0;
            }
        }
    }

    private void SpawnShotSound()
    {
        ShotSound shotSound = (ShotSound)ShotSoundScene.Instance();
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

	private void BulletShot(int damage)
	{
		Bullet bullet = (Bullet)BulletScene.Instance();
		bullet.Transform = gunTip.GlobalTransform;
		bullet.Rotate((float)GD.RandRange(-currentSpreading, currentSpreading));
		
		bullet.Shot(BulletSpeed, BulletSpeedRandomness, damage);
		GetParent().AddChild(bullet);
    }

    private void SpawnEjectedShell()
	{
        EjectedShell ejectedShell = (EjectedShell)EjectedShellScene.Instance();
        ejectedShell.Transform = bulletEjector.GlobalTransform;

		ejectedShell.Eject(velocity);
        GetParent().AddChild(ejectedShell);
    }

	private void SpawnBarrelFire()
	{
		BarrelFire barrelFire = (BarrelFire)BarrelFireScene.Instance();
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
			currentClip = ClipSize;
			UpdateAmmo(-ClipSize);
		}
	}

	private void SetAnimation()
	{
		if (punching)
        {
            shootingSprite.Animation = "melee";
            if (shootingSprite.Frame == 2)
			{
                SpawnPunchEffect();
            }
		}
		else if (reloading)
		{
			//shootingSprite.Animation = "reload";
		}
        else if (shooting)
        {
            shootingSprite.Animation = "shooting";
            if (Utilities.IsLastFrame(shootingSprite))
            {
                shooting = false;
            }
        }
        else
        {
            shootingSprite.Animation = "idle";
        }
        walkingSprite.GlobalRotationDegrees = Mathf.Rad2Deg(velocity.Angle()) + 90;
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
			currentSpreading = Spreading * 0.5f;
			line.AddPoint(Vector2.Zero);
			line.AddPoint(GetLocalMousePosition().Normalized() * 2000);
		}
		else
		{
			currentSpreading = Spreading;
		}
	}
}
