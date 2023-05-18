using Godot;
using System;

public class Bullet : Area2D
{
	private Vector2 velocity = new Vector2();
	private int Damage;

	public override void _PhysicsProcess(float delta)
	{
		Position += velocity * delta;
	}

	public void Shot(int bulletSpeed, float speedRandomness, int damage)
	{
		Damage = damage;
		velocity = new Vector2((float)GD.RandRange(bulletSpeed - speedRandomness, bulletSpeed + speedRandomness), 0).Rotated(Rotation);
	}

	public void OnBulletBodyEntered(Node body)
	{
		if (body is Zombie zombie)
        {
			zombie.ChangeHP(Damage);
            QueueFree();
		}
	}

	public void OnBulletScreenExited()
	{
		QueueFree();
	}
}
