using Godot;
using Shootem.Entities;

public partial class GenericWeapon : Pickable
{
    public int Damage;
    public int FireRate;
    public int ClipSize;
    public float ReloadTime;
    public int Spreading;
    public int BulletsPerShot;
    public int BulletSpeed;
    public int BulletSpeedRandomness;

    public Texture2D Texture;

    public void UpdateConfiguration()
    {
        GlobalSettings.Player.Damage = Damage;
        GlobalSettings.Player.FireRate = FireRate;
        GlobalSettings.Player.ClipSize = ClipSize;
        GlobalSettings.Player.Ammo = ClipSize * 3;
        GlobalSettings.Player.ReloadTime = ReloadTime;
        GlobalSettings.Player.Spreading = Spreading;
        GlobalSettings.Player.BulletsPerShot = BulletsPerShot;
        GlobalSettings.Player.BulletSpeed = BulletSpeed;
        GlobalSettings.Player.BulletSpeedRandomness = BulletSpeedRandomness;
    }
}
