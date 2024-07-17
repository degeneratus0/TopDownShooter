using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Zombie : CharacterBody2D
{
	[Signal] public delegate void ZombieKilledEventHandler(Vector2 position, bool dropped);

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

	float steerForce = 0.1f;
	int rayLenght = 30;
	int rayNum = 8;

	List<Vector2> rayDirections;
	List<float> interest;
	List<float> danger;

	private NavigationAgent2D navAgent;
	private Timer navTimer;
	private AnimationPlayer animPlayer;
	private Node attackSounds;
	private CollisionShape2D collision;
	private Area2D hitbox;
	private Control control;
	private HPBar hpBar;
	private Node2D animations;

	private Node2D rotatable;

	private PhysicsRayQueryParameters2D rayIntersectionParameters;
	private PhysicsDirectSpaceState2D spaceState;

	private PackedScene BloodEmitterScene;
	private PackedScene DeadSprite;

	private static Vector2 hpBarSize = new Vector2(32, 4);

	public override void _Ready()
	{
		BloodEmitterScene = GD.Load<PackedScene>("res://Entities/Zombie/ZombieObjects/BloodEmitter.tscn");
		DeadSprite = GD.Load<PackedScene>("res://Entities/Zombie/ZombieObjects/DeadZombie.tscn");

		GetNodes();
		InitZombie();
		hpBar.Init(health);

		interest = new List<float>(new float[rayNum]);
		danger = new List<float>(new float[rayNum]);
		rayDirections = new List<Vector2>(rayNum);
		for (int i = 0; i < rayNum; i++)
		{
			float angle = i * 2 * Mathf.Pi / rayNum;
			rayDirections.Add(Vector2.Right.Rotated(angle));
		}

		spaceState = GetWorld2D().DirectSpaceState;
		rayIntersectionParameters = new PhysicsRayQueryParameters2D
		{
			CollisionMask = 1,
			Exclude = { GetRid() }
		};
	}

	private void GetNodes()
	{
		navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
		navTimer = GetNode<Timer>("NavigationTimer");
		animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		attackSounds = GetNode<Node>("ZombieAttackSounds");
		collision = GetNode<CollisionShape2D>("CollisionShape2D");
		hitbox = GetNode<Area2D>("Hitbox");
		hpBar = GetNode<HPBar>("Control/HPBar");
		control = GetNode<Control>("Control");

		animations = GetNode<Node2D>("Rotatable/Animations");
		rotatable = GetNode<Node2D>("Rotatable");
	}

	private void InitZombie()
	{
		attackSpeed = GlobalSettings.Zombie.AttackSpeed;
		health = GlobalSettings.Zombie.HP + GD.RandRange(-GlobalSettings.Zombie.HP / 3, GlobalSettings.Zombie.HP / 3);
		float scaleFactor = (float)health / GlobalSettings.Zombie.HP;
		Scale *= scaleFactor;
		damage = (int)(GlobalSettings.Zombie.Damage * scaleFactor);
		dropChance = (float)GlobalSettings.Zombie.DropChance / 100;
		speed = GD.RandRange(GlobalSettings.Zombie.ZombieMinSpeed, GlobalSettings.Zombie.ZombieMaxSpeed);

		control.Size = hpBarSize;
	}

	public override void _Process(double delta)
	{
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

			Velocity = Velocity.Lerp(direction.Rotated(rotatable.Rotation) * speed, steerForce);

			rotatable.Rotation = Velocity.Angle();

			MoveAndSlide();
		}
	}

	public void SetPlayer(Player player)
	{
		this.player = player;
	}

	public void ChangeHP(int value)
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
				hpBar.Show();
			}
			hpBar.SetValue(health);

			if (health <= 0)
			{
				Kill();
			}
		}
	}

	public void Kill()
	{
		CreateBloodParticles();
		collision.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		GetNode<CollisionShape2D>("Hitbox/HitboxCollision").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		dying = true;
		hpBar.Hide();
		bool dropped = false;
		if (GD.Randf() < dropChance)
		{
			dropped = true;
		}
		animPlayer.Play("death");
		EmitSignal(SignalName.ZombieKilled, Position, dropped);
	}

	private void SetInterest()
	{
		Vector2 direction = ToLocal(navAgent.GetNextPathPosition()).Normalized();

		for (int i = 0; i < rayNum; i++)
		{
			var d = rayDirections[i].Rotated(rotatable.Rotation).Dot(direction);
			interest[i] = Mathf.Max(0, d);
		}
	}

	private void SetDanger()
	{
		for (int i = 0; i < rayNum; i++)
		{
			rayIntersectionParameters.From = Position;
			rayIntersectionParameters.To = Position + rayDirections[i].Rotated(rotatable.Rotation) * rayLenght;
			var result = spaceState.IntersectRay(rayIntersectionParameters);
			danger[i] = result.Any() ? 1 : 0;
		}
	}

	private Vector2 SetDirection()
	{
		for (int i = 0; i < rayNum; i++)
		{
			if (danger[i] > 0)
			{
				interest[i] = 0;
			}
		}

		Vector2 direction = Vector2.Zero;
		for (int i = 0; i < rayNum; i++)
		{
			direction += rayDirections[i] * interest[i];
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
			deadSprite.Rotation = rotatable.Rotation;
			deadSprite.Scale = Scale;
			GetParent().AddChild(deadSprite);
			QueueFree();
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
		BloodEmitter bloodEmitter = (BloodEmitter)BloodEmitterScene.Instantiate();
        bloodEmitter.Position = Position;
        bloodEmitter.Finished += QueueFree;
        GetParent().AddChild(bloodEmitter);
        bloodEmitter.Emitting = true;
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

	private void CauseDamage()
	{
		if (canAttack)
		{
			player.ChangeHP(-damage);
		}
	}

	private void OnHitboxAreaEntered(Area2D area)
	{
		if (area is Bullet)
		{
			ChangeHP(-player.Damage);
		}
		else if (area == player.MeleeArea)
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
