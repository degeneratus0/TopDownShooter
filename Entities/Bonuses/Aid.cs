using Godot;

public partial class Aid : Bonus
{
	public Aid()
	{
		Color = Colors.White;
	}

	public override void DoBonus(Player player)
	{
		player.ChangeHP(player.MaxHealth);
	}
}
