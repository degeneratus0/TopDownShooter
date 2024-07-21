public static class GlobalSettings
{
	public static class Player
	{
		public static int Damage = 60;
		public static int MaxHP = 100;
		public static int Speed = 120;  
		public static int FireRate = 1600;
		public static int ClipSize = 100;
		public static int Ammo = 500;
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
			Speed = 120;
			IsInvincible = false;
		}
	}

	public static class Zombie
	{
		public static float AttackSpeed = 1;
		public static int Damage = 25;
		public static int HP = 100;
		public static int AverageSpeed = 120;
		public static int DropChance = 3;
	}

	public static class Difficulty
	{
		public static int WaveBase = 50;
		public static float WaveMultiplier = 1.2f;

		public static float ZombieSpawnRate = 1;
		public static int ZombieSpawnRateIncrement = 2;
		public static float MaxSpawnRate = 10;
		public static int ZombiesToIncrement = 5;
		public static bool ShowSpawnRate = false;

		public static void SetDefault()
		{
			Zombie.Damage = 25;
			Zombie.HP = 100;
			Zombie.AverageSpeed = 120;
			Zombie.DropChance = 3;

			ZombieSpawnRate = 1;
			ZombieSpawnRateIncrement = 2;
			MaxSpawnRate = 10;
			ZombiesToIncrement = 5;
			ShowSpawnRate = false;

		}
	}
}
