using Godot;

public partial class Armor : Bonus
{
	public Armor()
	{
		Color = Colors.Blue;
	}

	public override void DoBonus(Player player)
	{
		player.AddArmor();
	}
}
