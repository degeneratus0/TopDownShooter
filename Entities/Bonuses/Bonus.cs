using Shootem.Entities;

public partial class Bonus : Pickable
{
    private BonusType bonusType;

    public Bonus(BonusType bonusType)
    {
        this.bonusType = bonusType;
    }

    public void DoBonus(Player player)
    {
        switch (bonusType)
        {
            case BonusType.BulletPack:
                player.UpdateAmmo(player.ClipSize);
                break;
            case BonusType.Aid:
                player.ChangeHP(player.MaxHP);
                break;
            case BonusType.DamageUp:
                player.DamageUp(10, 5);
                break;
        }
    }
}

public enum BonusType
{
    BulletPack,
    Aid,
    DamageUp
}

