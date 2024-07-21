using Godot;

public partial class Piercing : Bonus
{
	public Piercing()
	{
		Color = Colors.GreenYellow;
	}

	public override void DoBonus(Player player)
	{
		player.InvokeTimedBonus(this, 10, () => UndoBonus(player));
		player.Piercing = true;
	}

	public override void UndoBonus(Player player)
	{
		player.Piercing = false;
	}
}
