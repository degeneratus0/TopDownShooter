using Godot;

public partial class Bullet : Area2D
{
	private Vector2 velocity = new Vector2();
	private bool piercing = false;

    public override void _Ready()
    {
        Timer timer = new Timer();
        timer.Autostart = true;
        timer.OneShot = true;
        timer.WaitTime = 0.5;
        timer.Timeout += QueueFree;
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
		velocity = new Vector2((float)GD.RandRange(bulletSpeed - speedRandomness, bulletSpeed + speedRandomness), 0).Rotated(Rotation);
    } 

	public void OnBulletBodyEntered(Node body)
    {
		if (!piercing)
        {
            QueueFree();
        }
	}
}
