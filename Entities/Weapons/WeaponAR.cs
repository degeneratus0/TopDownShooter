using Godot;

public partial class WeaponAR : GenericWeapon
{
	Sprite2D sprite;

	public override void _Ready()
	{
		base._Ready();
		Damage = 20;
		FireRate = 800;
		ClipSize = 30;
		ReloadTime = 3;
		Spreading = 8;
		BulletsPerShot = 1;
		BulletSpeed = 750;
		BulletSpeedRandomness = 50;

		sprite = GetNode<Sprite2D>("Sprite2D");
		Texture = sprite.Texture;
	}
}

