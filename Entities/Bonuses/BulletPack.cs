using Godot;

public partial class BulletPack : Bonus
{
	public BulletPack()
	{
		Color = Colors.Yellow;
	}

	public override void DoBonus(Player player)
	{
		player.UpdateAmmo(GlobalSettings.Player.ClipSize);
	}
}
