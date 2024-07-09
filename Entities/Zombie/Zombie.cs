using Godot;
using Godot.Collections;
using System;
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
		HPBar.Init(health);
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
		attackSpeed = GlobalSettings.Zombie.AttackSpeed;
		health = GlobalSettings.Zombie.HP + GD.RandRange(-GlobalSettings.Zombie.HP / 3, GlobalSettings.Zombie.HP / 3);
		float scaleFactor = (float)health / GlobalSettings.Zombie.HP;
		Scale *= scaleFactor;
		damage = (int)(GlobalSettings.Zombie.Damage * scaleFactor);
		dropChance = (float)GlobalSettings.Zombie.DropChance / 100;
		speed = GD.RandRange(GlobalSettings.Zombie.ZombieMinSpeed, GlobalSettings.Zombie.ZombieMaxSpeed);
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
			Vector2 direction = ToLocal(navAgent.GetNextPathPosition()).Normalized();

			animations.Rotation = direction.Angle();

			Velocity = direction * speed;
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
		//if (Position.DistanceTo(player.Position) > 2000)
		//{
		//    QueueFree();
		//}
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

		if (health > 0)
		{
			health += value;
			if (health < GlobalSettings.Zombie.HP)
			{
				HPBar.Show();
			}
			HPBar.SetValue(health);

			if (health <= 0)
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
		CreateBloodParticles();
		collision.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		GetNode<CollisionShape2D>("Hitbox/HitboxCollision").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		dying = true;
		HPBar.Hide();
		bool dropped = false;
		if (GD.Randf() < dropChance)
		{
			dropped = true;
		}
		animPlayer.Play("death");
		EmitSignal(SignalName.ZombieKilled, Position, dropped);
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
			ChangeHP(-45);
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
