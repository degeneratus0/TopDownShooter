public static class GlobalSettings
{
    public static class Player
    {
        public static int Damage = 10;
        public static int MaxHP = 100;
        public static int Speed = 120;  
        public static int FireRate = 1200;
        public static int ClipSize = 100;
        public static int Ammo = 300;
        public static float ReloadTime = 2f;
        public static int Spreading = 5;
        public static int BulletsPerShot = 1;
        public static int BulletSpeed = 750;
        public static int BulletSpeedRandomness = 50;
        public static bool IsInvincible = false;

        public static void SMGPreset()
        {
            Damage = 10;
            FireRate = 900;
            ClipSize = 30;
            Ammo = 90;
            ReloadTime = 1.5f;
            Spreading = 5;
            BulletsPerShot = 1;
            BulletSpeed = 750;
            BulletSpeedRandomness = 50;
    }

        public static void ShotgunPreset()
        {
            Damage = 4;
            FireRate = 120;
            ClipSize = 10;
            Ammo = 50;
            ReloadTime = 2.5f;
            Spreading = 20;
            BulletsPerShot = 15;
            BulletSpeed = 650;
            BulletSpeedRandomness = 150;
        }

        public static void SetDefault()
        {
            SMGPreset();
            Speed = 250;
            IsInvincible = false;
        }
    }

    public static class Zombie
    {
        public static float AttackSpeed = 1;
        public static int Damage = -25;
        public static int HP = 100;
        public static int ZombieMinSpeed = 80;
        public static int ZombieMaxSpeed = 160;
        public static int DropChance = 10;
    }

    public static class Difficulty
    {
        public static float ZombieSpawnRate = 1;
        public static int ZombieSpawnRateIncrement = 2;
        public static float MaxSpawnRate = 10;
        public static int ZombiesToIncrement = 5;
        public static bool ShowSpawnRate = false;

        public static void SetDefault()
        {
            Zombie.Damage = -25;
            Zombie.HP = 100;
            Zombie.ZombieMinSpeed = 100;
            Zombie.ZombieMaxSpeed = 200;
            Zombie.DropChance = 10;

            ZombieSpawnRate = 1;
            ZombieSpawnRateIncrement = 2;
            MaxSpawnRate = 10;
            ZombiesToIncrement = 5;
            ShowSpawnRate = false;

        }
    }
}
