using Godot;

namespace shootem.Objects.Bonuses
{
    public class Bonus : Area2D
    {
        [Signal] public delegate void BonusPicked(Bonus bonus);

        private BonusType bonusType;

        public Bonus(BonusType bonusType)
        {
            this.bonusType = bonusType;
            Connect("body_entered", this, nameof(OnBonusBodyEntered));
        }

        public void OnBonusBodyEntered(Node body)
        {
            if (body is Player)
            {
                EmitSignal(nameof(BonusPicked), this);
                QueueFree();
            }
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
                    player.DamageUp(5);
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
}
