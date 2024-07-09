using Godot;
using Shootem.Entities;
using System.Collections.Generic;

public partial class Bonus : Pickable
{
	private BonusType bonusType;
	public static List<PackedScene> BonusesScenes = new List<PackedScene>()
		{
			GD.Load<PackedScene>("res://Entities/Bonuses/BulletPack.tscn"),
			GD.Load<PackedScene>("res://Entities/Bonuses/Aid.tscn"),
			GD.Load<PackedScene>("res://Entities/Bonuses/DamageUp.tscn"),
			GD.Load<PackedScene>("res://Entities/Bonuses/Armor.tscn"),
			GD.Load<PackedScene>("res://Entities/Bonuses/SpeedUp.tscn"),
			GD.Load<PackedScene>("res://Entities/Bonuses/Piercing.tscn")
		};

	public Bonus(BonusType bonusType)
	{
		this.bonusType = bonusType;
	}

	public static Bonus GetRandomBonus()
	{
		PackedScene bonusScene = Utilities.GetRandomSceneFromList(Bonus.BonusesScenes);
		return (Bonus)bonusScene.Instantiate();
	}

	public void DoBonus(Player player)
	{
		switch (bonusType)
		{
			case BonusType.BulletPack:
				player.UpdateAmmo(GlobalSettings.Player.ClipSize);
				break;
			case BonusType.Aid:
				player.ChangeHP(player.MaxHP);
				break;
			case BonusType.Armor:
				player.AddArmor();
				break;
			case BonusType.DamageUp:
				player.DamageUp(10, 5);
				break;
			case BonusType.SpeedUp:
				player.SpeedUp(10, 1.5);
				break;
			case BonusType.Piercing:
				player.PierceUp(10);
				break;
		}
	}
}

public enum BonusType
{
	BulletPack,
	Aid,
	Armor,
	DamageUp,
	SpeedUp,
	Piercing
}

