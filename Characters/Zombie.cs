using Godot;

public class Zombie : KinematicBody2D
{
    [Signal] public delegate void ZombieKilled(Vector2 position, bool dropped);

    private float AttackSpeed;
    private int Damage;
    private int HP;
    private float DropChance;

    private int speed;
    private float fromAttack = 0;
    private bool attacking = false;
    private Player player = null;

    private AnimatedSprite sprite;
    private Node AttackSounds;
    private RayCast2D attackRayCast;
    private RayCast2D tryAttackRayCast;
    private HPBar HPBar;

    private PackedScene BloodParticlesScene;

    public override void _Ready()
    {
        BloodParticlesScene = GD.Load<PackedScene>("res://Objects/MobObjects/BloodParticles.tscn");

        GetNodes();
        InitZombie();
        HPBar.Init(HP);
    }

    private void GetNodes()
    {
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        AttackSounds = GetNode<Node>("ZombieAttackSounds");
        attackRayCast = GetNode<RayCast2D>("AttackRayCast");
        tryAttackRayCast = GetNode<RayCast2D>("TryAttackRayCast");
        HPBar = GetNode<HPBar>("HPBar");
    }

    private void InitZombie()
    {
        AttackSpeed = GlobalSettings.Zombie.AttackSpeed;
        Damage = GlobalSettings.Zombie.Damage;
        HP = GlobalSettings.Zombie.HP;
        DropChance = (float)GlobalSettings.Zombie.DropChance / 100;
        speed = (int)GD.RandRange(GlobalSettings.Zombie.ZombieMinSpeed, GlobalSettings.Zombie.ZombieMaxSpeed);
    }

    public void ChangeHP(int value)
    {
        if (value < 0)
        {
            AddChild(BloodParticlesScene.Instance());
        }

        HP += value;
        HPBar.SetValue(HP);

        if (HP <= 0)
        {
            Kill();
        }
    }

    public override void _Process(float delta)
    {
        if (fromAttack < AttackSpeed)
        {
            fromAttack += delta;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (player == null)
        {
            return;
        }

        Vector2 velocity = Position.DirectionTo(player.Position);
        LookAt(player.Position);
        MoveAndCollide(velocity * speed * delta);

        if (attacking && Utilities.IsLastFrame(sprite))
        {
            attacking = false;
            sprite.Animation = "walk";
        }

        if (!attacking && fromAttack >= AttackSpeed && tryAttackRayCast.IsColliding() && tryAttackRayCast.GetCollider() is Player)
        {
            fromAttack = 0;
            attacking = true;
            sprite.Animation = "attack";
            AttackSounds.GetChild<AudioStreamPlayer2D>(Utilities.RandNum(AttackSounds.GetChildCount())).Play();
            if (attackRayCast.IsColliding() && attackRayCast.GetCollider() is Player player)
            {
                player.ChangeHP(Damage);
            }
        }
    }

    public void Kill()
    {
        bool dropped = false;
        if (GD.Randf() < DropChance)
        {
            dropped = true;
        }
        EmitSignal(nameof(ZombieKilled), Position, dropped);
        QueueFree();
    }

    public void SetPlayer(Player player)
    {
        this.player = player;
    }
}
