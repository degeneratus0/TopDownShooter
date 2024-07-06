using Godot;

public partial class Bullet : Area2D
{
	private Vector2 velocity = new Vector2();

	public override void _PhysicsProcess(double delta)
	{
		Position += velocity * (float)delta;
	}

	public void Shot(int bulletSpeed, float speedRandomness)
	{
		velocity = new Vector2((float)GD.RandRange(bulletSpeed - speedRandomness, bulletSpeed + speedRandomness), 0).Rotated(Rotation);
	} 

	public void OnBulletBodyEntered(Node body)
    {
        QueueFree();
	}
}
