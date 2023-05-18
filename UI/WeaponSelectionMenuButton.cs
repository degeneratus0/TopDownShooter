using Godot;
using System;

public class WeaponSelectionMenuButton : MenuButton
{
    private PopupMenu popup;

    public override void _Ready()
    {
        popup = GetPopup();
        popup.AddItem("SMG");
        popup.AddItem("Shotgun");
        popup.Connect("index_pressed", this, nameof(WeaponSelected));
    }

    private void WeaponSelected(int index)
    {
        switch (popup.GetItemText(index))
        {
            case "SMG":
                GlobalSettings.Player.SMGPreset();
                break;
            case "Shotgun":
                GlobalSettings.Player.ShotgunPreset();
                break;
        }
    }
}
