using Godot;
using Shootem.Entities;
using System.Collections.Generic;

public partial class Bonus : Pickable
{
	public Color Color;

	private static List<PackedScene> bonusesScenes = new List<PackedScene>()
		{
			GD.Load<PackedScene>("res://Entities/Bonuses/BulletPack.tscn"),
			GD.Load<PackedScene>("res://Entities/Bonuses/Aid.tscn"),
			GD.Load<PackedScene>("res://Entities/Bonuses/DamageUp.tscn"),
			GD.Load<PackedScene>("res://Entities/Bonuses/Armor.tscn"),
			GD.Load<PackedScene>("res://Entities/Bonuses/SpeedUp.tscn"),
			GD.Load<PackedScene>("res://Entities/Bonuses/Piercing.tscn")
		};

	public static Bonus GetRandomBonus()
	{
		PackedScene bonusScene = Utilities.GetRandomSceneFromList(bonusesScenes);
		return (Bonus)bonusScene.Instantiate();
	}

	public virtual void DoBonus(Player player)
	{
		return;
	}

	public virtual void UndoBonus(Player player)
	{
		return;
	}
}

