using Godot;

public partial class Player : CharacterBody2D
{
	[Signal] public delegate void PlayerKilledEventHandler();
	[Signal] public delegate void AmmoUpdatedEventHandler(int currentAmmo, int maxAmmo);

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
	public float Spreading
    {
        get
        {
            return currentSpreading;
        }
        set
        {
            currentSpreading = value;
        }
    }
	public int BulletsPerShot;
	public int BulletSpeed;
	public int BulletSpeedRandomness;
	public bool IsInvincible;
    public int HP;

    public Area2D Hitbox;
    public Area2D MeleeArea;

    private int currentDamage;
    private float currentSpreading;
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
    private TextureProgressBar hpBar;
    private TextureProgressBar damageUpBar;
    private TextureProgressBar reloadBar;
    private Timer damageUpTimer;
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
        Spreading = Mathf.DegToRad(GlobalSettings.Player.Spreading);
        BulletsPerShot = GlobalSettings.Player.BulletsPerShot;
		BulletSpeed = GlobalSettings.Player.BulletSpeed;
		BulletSpeedRandomness = GlobalSettings.Player.BulletSpeedRandomness;
		IsInvincible = GlobalSettings.Player.IsInvincible;

        InitUi();

        fromReload = 0;

        UpdateAmmo(0);
    }

    private void InitUi()
    {
        hpBar.MaxValue = MaxHP;
        hpBar.Value = MaxHP;
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
        hpBar = GetNode<TextureProgressBar>("ControlsContainer/HPBar");
        damageUpBar = GetNode<TextureProgressBar>("ControlsContainer/DamageUpBar");
        reloadBar = GetNode<TextureProgressBar>("ControlsContainer/ReloadBar");
        damageUpTimer = GetNode<Timer>("DamageUpTimer");
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
        if (!IsInvincible)
        {
            isDead = true;
            EmitSignal(SignalName.PlayerKilled);
            Hide();
            GetNode<CollisionShape2D>("Hitbox/HitboxCollision").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
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

	public void DamageUp(int seconds, int multiplier)
	{
		currentDamage = GlobalSettings.Player.Damage * multiplier;
        damageUpTimer.WaitTime = seconds;
        damageUpTimer.Start();
        damageUpBar.MaxValue = damageUpTimer.WaitTime;
        damageUpBar.Value = damageUpTimer.WaitTime;
        damageUpBar.Show();
    }

	public void OnDamageUpTimerTimeout()
	{
		currentDamage = GlobalSettings.Player.Damage;
        damageUpBar.Hide();
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
        damageUpBar.Value = damageUpTimer.TimeLeft;

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
		bullet.Rotate((float)GD.RandRange(-currentSpreading, currentSpreading));
		
		bullet.Shot(BulletSpeed, BulletSpeedRandomness);
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
			currentSpreading = Spreading * 0.5f;
			line.AddPoint(Vector2.Zero);
			line.AddPoint(GetLocalMousePosition().Normalized() * 500);
		}
		else
		{
			currentSpreading = Spreading;
		}
	}
}
