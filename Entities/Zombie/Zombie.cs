using Godot;
using System.Collections.Generic;

public partial class Zombie : CharacterBody2D
{
	[Signal] public delegate void ZombieKilledEventHandler(Zombie zombie, bool dropped);

	private float attackSpeed;
	private int damage;
	private int health;
	private float dropChance;
	private int speed;

	private double fromAttack = 0;
	private bool attacking = false;
	private bool canAttack = false;
	private bool dying = false;
	private Player player = null;

	float steerForce = 0.2f;
	int rayLenght = 20;
	int rayNum = 8;

	List<RayCast2D> rays;
	List<float> interest;
	List<bool> danger;

	private NavigationAgent2D navAgent;
	private Timer navTimer;
	private AnimationPlayer animPlayer;
	private Node attackSounds;
	private TextureProgressBar healthBar;
	private Node2D controlsContainer;

	private CollisionShape2D collision;
	private CollisionShape2D hitbox;

	private PackedScene BloodEmitterScene;
	private PackedScene DeadSprite;

	public override void _Ready()
	{
		BloodEmitterScene = GD.Load<PackedScene>("res://Entities/Zombie/ZombieObjects/BloodEmitter.tscn");
		DeadSprite = GD.Load<PackedScene>("res://Entities/Zombie/ZombieObjects/DeadZombie.tscn");

		GetNodes();
		InitZombie();

		interest = new List<float>(new float[rayNum]);
		danger = new List<bool>(new bool[rayNum]);
		rays = new List<RayCast2D>(rayNum);
		for (int i = 0; i < rayNum; i++)
		{
			float angle = i * 2 * Mathf.Pi / rayNum;
			RayCast2D ray = new RayCast2D();
			ray.TargetPosition = Vector2.Right.Rotated(angle) * rayLenght;
			ray.CollisionMask = 1; //Obstacles
			ray.Enabled = true;
			ray.ExcludeParent = true;
			AddChild(ray);
			rays.Add(ray);
		}
	}

	private static Dictionary<string, NodePath> nodePaths = new Dictionary<string, NodePath>
	{
		{ "NavigationAgent2D", "NavigationAgent2D" },
		{ "NavigationTimer", "NavigationTimer" },
		{ "AnimationPlayer", "AnimationPlayer" },
		{ "ZombieAttackSounds", "ZombieAttackSounds" },
		{ "Hitbox/HitboxCollision", "Hitbox/HitboxCollision" },
		{ "CollisionShape2D", "CollisionShape2D" },
		{ "Rotatable", "Rotatable" },
		{ "ControlsContainer/HealthBar", "ControlsContainer/HealthBar" },
		{ "ControlsContainer", "ControlsContainer" }
	};

	private void GetNodes()
	{
		navAgent = GetNode<NavigationAgent2D>(nodePaths["NavigationAgent2D"]);
		navTimer = GetNode<Timer>(nodePaths["NavigationTimer"]);
		animPlayer = GetNode<AnimationPlayer>(nodePaths["AnimationPlayer"]);
		attackSounds = GetNode<Node>(nodePaths["ZombieAttackSounds"]);
		healthBar = GetNode<TextureProgressBar>(nodePaths["ControlsContainer/HealthBar"]);
		controlsContainer = GetNode<Node2D>(nodePaths["ControlsContainer"]);

		hitbox = GetNode<CollisionShape2D>(nodePaths["Hitbox/HitboxCollision"]);
		collision = GetNode<CollisionShape2D>(nodePaths["CollisionShape2D"]);
	}

	private void InitZombie()
	{
		attackSpeed = GlobalSettings.Zombie.AttackSpeed;
		health = GlobalSettings.Zombie.HP + GD.RandRange(-GlobalSettings.Zombie.HP / 3, GlobalSettings.Zombie.HP / 3);
		float scaleFactor = (float)health / GlobalSettings.Zombie.HP;
		Scale *= Mathf.Pow(scaleFactor, 1/2.5f);
		damage = (int)(GlobalSettings.Zombie.Damage * scaleFactor);
		dropChance = (float)GlobalSettings.Zombie.DropChance / 100;
		speed = (int)(GlobalSettings.Zombie.AverageSpeed / scaleFactor);

		healthBar.MaxValue = health;
		healthBar.Value = health;

		attacking = false;
		canAttack = false;
		dying = false;

		hitbox.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
		collision.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
	}

	public override void _Process(double delta)
	{
		controlsContainer.GlobalRotation = 0;

		if (player == null || dying)
		{
			return;
		}
		if (fromAttack < attackSpeed)
		{
			fromAttack += delta;
		}
		Attack();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!navAgent.IsNavigationFinished())
		{
			SetInterest();
			SetDanger();
			Vector2 direction = SetDirection();

			Velocity = Velocity.Lerp(direction.Rotated(Rotation) * speed, steerForce);
			Rotation = Velocity.Angle();

			MoveAndSlide();
		}
	}

	public void SetPlayer(Player player)
	{
		this.player = player;
	}

	public void Hit()
	{
		ChangeHP(-player.Damage);
	}

	private void ChangeHP(int value)
	{
		if (value < 0)
		{
			CreateBloodParticles();
		}

		if (health > 0)
		{
			health += value;
			if (health < GlobalSettings.Zombie.HP)
			{
				healthBar.Show();
			}
			healthBar.Value = health;

			if (health <= 0 && !dying)
			{
				dying = true;
				Kill();
			}
		}
	}

	public void Kill()
	{
		hitbox.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		collision.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);

		healthBar.Hide();
		bool dropped = false;
		if (GD.Randf() < dropChance)
		{
			dropped = true;
		}
		animPlayer.Play("death");
		EmitSignal(SignalName.ZombieKilled, this, dropped);

		CreateBloodParticles();
	}

	private void SetInterest()
	{
		Vector2 direction = ToLocal(navAgent.GetNextPathPosition()).Normalized();

		for (int i = 0; i < rayNum; i++)
		{
			var d = rays[i].TargetPosition.Dot(direction);
			interest[i] = Mathf.Max(0, d);
		}
	}

	private void SetDanger()
	{
		for (int i = 0; i < rayNum; i++)
		{
			bool result = false;

			if (rays[i].IsColliding())
			{
				result = true;
			}
			danger[i] = result;
		}
	}

	private Vector2 SetDirection()
	{
		for (int i = 0; i < rayNum; i++)
		{
			if (danger[i])
			{
				interest[i] = 0;
			}
		}

		Vector2 direction = Vector2.Zero;
		for (int i = 0; i < rayNum; i++)
		{
			direction += rays[i].TargetPosition * interest[i];
		}

		return direction.Normalized();
	}

	private void OnScreenExited()
	{
		navTimer.WaitTime = 1;
	}

	private void OnScreenEntered()
	{
		navTimer.WaitTime = 0.1;
	}

	private void OnAnimationFinished(StringName animation)
	{
		if (attacking && animation == "attack")
		{
			attacking = false;
			animPlayer.Play("run");
		}
		if (animation == "death")
		{
			animPlayer.Play("RESET");
			Sprite2D deadSprite = (Sprite2D)DeadSprite.Instantiate();
			deadSprite.Position = Position;
			deadSprite.Rotation = Rotation;
			deadSprite.Scale = Scale;
			GetParent().AddChild(deadSprite);
			InitZombie();
			ScenePool<Zombie>.Release(this);
			animPlayer.Play("run");
		}
	}

	private void OnNavigationTimerTimeout()
	{
		//if (Position.DistanceTo(player.Position) > 2000)
		//{
		//    QueueFree();
		//}
		if (!dying)
		{
			navAgent.TargetPosition = player.GlobalPosition;
		}
	}

	private void CreateBloodParticles()
	{
		var bloodEmitter = (GpuParticles2D)BloodEmitterScene.Instantiate();
		bloodEmitter.Position = Position;
		bloodEmitter.Finished += bloodEmitter.QueueFree;
		bloodEmitter.Emitting = true;
		GetParent().AddChild(bloodEmitter);
	}

	private void Attack()
	{
		if (canAttack && fromAttack >= attackSpeed)
		{
			fromAttack = 0;
			attacking = true;
			animPlayer.Play("attack");
			attackSounds.GetChild<AudioStreamPlayer2D>(Utilities.RandNum(attackSounds.GetChildCount())).Play();
		}
	}

	private void DealDamage()
	{
		if (canAttack)
		{
			player.ChangeHP(-damage);
		}
	}

	private void OnHitboxAreaEntered(Area2D area)
	{
		if (area == player.MeleeArea)
		{
			ChangeHP(-45);
		}
	}

	private void OnAttackAreaEntered(Area2D area)
	{
		if (area == player.Hitbox)
		{
			canAttack = true;
		}
	}

	private void OnAttackAreaExited(Area2D area)
	{
		if (area == player.Hitbox)
		{
			canAttack = false;
		}
	}
}
