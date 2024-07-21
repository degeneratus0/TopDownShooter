using Godot;

public partial class DamageUp : Bonus
{
	public DamageUp()
	{
		Color = Colors.DarkRed;
	}

	public override void DoBonus(Player player)
	{
		player.InvokeTimedBonus(this, 10, () => UndoBonus(player));
		player.Damage = GlobalSettings.Player.Damage * 5;
	}

	public override void UndoBonus(Player player)
	{

		player.Damage = GlobalSettings.Player.Damage;
	}
}
