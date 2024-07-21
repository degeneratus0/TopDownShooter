using Godot;

public partial class Bullet : Area2D
{
	private Vector2 velocity = new Vector2();
	private bool piercing = false;
	private bool hit = false;

	private Timer timer;

	public override void _Ready()
	{
		timer = new Timer();
		timer.OneShot = true;
		timer.WaitTime = 0.5;
		timer.Timeout += Dispose;
		AddChild(timer);
		timer.Start();
	}

	public override void _PhysicsProcess(double delta)
	{
		Position += velocity * (float)delta;
	}

	public void Shot(int bulletSpeed, float speedRandomness, bool piercing)
	{
		this.piercing = piercing;
		hit = false;
		velocity = new Vector2((float)GD.RandRange(bulletSpeed - speedRandomness, bulletSpeed + speedRandomness), 0).Rotated(Rotation);
	} 

	public void OnBulletBodyEntered(Node body)
	{
		if (body is Zombie zombie && (!hit || piercing))
		{
			hit = true;
			zombie.Hit();
		}
		if (!piercing)
		{
			Dispose();
		}
	}

	public new void Dispose()
	{
		ScenePool<Bullet>.Release(this);
	}
}
