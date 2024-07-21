using Godot;

public partial class SpeedUp : Bonus
{
	public SpeedUp()
	{
		Color = Colors.Yellow;
	}

	public override void DoBonus(Player player)
	{
		player.InvokeTimedBonus(this, 10, () => UndoBonus(player));
		player.Speed = (int)(GlobalSettings.Player.Speed * 1.5);
	}

	public override void UndoBonus(Player player)
	{

		player.Speed = GlobalSettings.Player.Speed;
	}
}
