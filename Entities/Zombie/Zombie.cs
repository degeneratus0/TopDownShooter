using Godot;

public partial class Zombie : CharacterBody2D
{
	[Signal] public delegate void ZombieKilledEventHandler(Vector2 position, bool dropped);

	private float AttackSpeed;
	private int Damage;
	private int HP;
	private float DropChance;
	private int Speed;

	private double fromAttack = 0;
	private bool attacking = false;
	private bool canAttack = false;
	private bool dying = false;
	private Player player = null;

	private NavigationAgent2D navAgent;
	private Timer navTimer;
	private AnimationPlayer animPlayer;
	private Node attackSounds;
	private CollisionShape2D collision;
	private Area2D hitbox;
	private HPBar HPBar;
	private Node2D animations;

	private PackedScene BloodEmitterScene;
	private PackedScene DeadSprite;

	public override void _Ready()
	{
		BloodEmitterScene = GD.Load<PackedScene>("res://Entities/Zombie/ZombieObjects/BloodEmitter.tscn");
		DeadSprite = GD.Load<PackedScene>("res://Entities/Zombie/ZombieObjects/DeadZombie.tscn");

		GetNodes();
		InitZombie();
		HPBar.Init(HP);
		float scaleFactor = (float)HP / GlobalSettings.Zombie.HP;
		Scale *= scaleFactor;
	}

	private void GetNodes()
	{
		navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
		navTimer = GetNode<Timer>("NavigationTimer");
		animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		attackSounds = GetNode<Node>("ZombieAttackSounds");
		collision = GetNode<CollisionShape2D>("CollisionShape2D");
		hitbox = GetNode<Area2D>("Hitbox");
		HPBar = GetNode<HPBar>("HPBar");

		animations = GetNode<Node2D>("Animations");
	}

	private void InitZombie()
	{
		AttackSpeed = GlobalSettings.Zombie.AttackSpeed;
		Damage = GlobalSettings.Zombie.Damage;
		HP = GlobalSettings.Zombie.HP + GD.RandRange(-GlobalSettings.Zombie.HP / 3, GlobalSettings.Zombie.HP / 3);
		DropChance = (float)GlobalSettings.Zombie.DropChance / 100;
		Speed = GD.RandRange(GlobalSettings.Zombie.ZombieMinSpeed, GlobalSettings.Zombie.ZombieMaxSpeed);
	}

	public override void _Process(double delta)
	{
		if (player == null || dying)
		{
			return;
		}
		if (fromAttack < AttackSpeed)
		{
			fromAttack += delta;
		}
		Attack();
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 direction = ToLocal(navAgent.GetNextPathPosition()).Normalized();

		animations.Rotation = direction.Angle();
		if (!navAgent.IsNavigationFinished())
		{
			Velocity = direction * Speed;
			MoveAndSlide();
		}
	}

	private void OnScreenExited()
	{
		navTimer.WaitTime = 1;
	}

	private void OnScreenEntered()
	{
		navTimer.WaitTime = 0.1;
	}

	private void OnAnimationFinished(string animation)
	{
		if (attacking && animation == "attack")
		{
			attacking = false;
			animPlayer.Play("run");
		}
		if (animation == "death")
		{
			Sprite2D deadSprite = (Sprite2D)DeadSprite.Instantiate();
			deadSprite.Position = Position;
			deadSprite.Rotation = animations.Rotation;
			deadSprite.Scale = Scale;
			GetParent().AddChild(deadSprite);
			QueueFree();
		}
	}

	private void OnNavigationTimerTimeout()
	{
		if (Position.DistanceTo(player.Position) > 2000)
		{
			QueueFree();
		}
		if (!dying)
		{
			navAgent.TargetPosition = player.GlobalPosition;
		}
	}

	public void ChangeHP(int value)
	{
		if (value < 0)
		{
			CreateBloodParticles();
		}

		if (HP > 0)
		{
			HP += value;
			if (HP < GlobalSettings.Zombie.HP)
			{
				HPBar.Show();
			}
			HPBar.SetValue(HP);

			if (HP <= 0)
			{
				Kill();
			}
		}
	}

	private void CreateBloodParticles()
	{
		BloodEmitter bloodEmitter = (BloodEmitter)BloodEmitterScene.Instantiate();
		bloodEmitter.Position = Position;
		GetParent().AddChild(bloodEmitter);
	}

	public void Kill()
	{
		collision.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		GetNode<CollisionShape2D>("Hitbox/HitboxCollision").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		dying = true;
		HPBar.Hide();
		bool dropped = false;
		if (GD.Randf() < DropChance)
		{
			dropped = true;
		}
		animPlayer.Play("death");
		EmitSignal(SignalName.ZombieKilled, Position, dropped);
	}

	private void Attack()
	{
		if (canAttack && fromAttack >= AttackSpeed)
		{
			fromAttack = 0;
			attacking = true;
			animPlayer.Play("attack");
			attackSounds.GetChild<AudioStreamPlayer2D>(Utilities.RandNum(attackSounds.GetChildCount())).Play();
		}
	}

	private void CauseDamage()
	{
		if (canAttack)
		{
			player.ChangeHP(Damage);
		}
	}

	public void SetPlayer(Player player)
	{
		this.player = player;
	}

	private void OnHitboxAreaEntered(Area2D area)
	{
		if (area == player.Hitbox)
		{
			canAttack = true;
		}
		else if (area is Bullet)
		{
			ChangeHP(-player.Damage);
		}
		else if (area == player.MeleeArea)
		{
			CreateBloodParticles();
			Kill();
		}
	}

	private void OnHitboxAreaExited(Area2D area)
	{
		if (area == player.Hitbox)
		{
			canAttack = false;
		}
	}
}
