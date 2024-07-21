using Godot;

public partial class UI : CanvasLayer
{
	private int score = 0;

	private Label scoreLabel;
	private Label ammoLabel;
	private Label spawnRateLabel;
	private Label zombikov;
	private TextureRect weaponTexture;

	private const string path = "Control/MarginContainer/";

	public override void _Ready()
	{
		scoreLabel = GetNode<Label>(path + "ScoreLabel");
		weaponTexture = GetNode<TextureRect>(path + "HBox/WeaponTexture");
		ammoLabel = GetNode<Label>(path + "HBox/AmmoLabel");
		spawnRateLabel = GetNode<Label>(path + "SpawnRateLabel");
		zombikov = GetNode<Label>(path + "Zombikov");
	}

	public void UpdateScoreLabel(int value)
	{
		score += value;
		scoreLabel.Text = $"Score: {score}";
	}

	public void UpdateWeaponTexture(Texture2D texture)
	{
		weaponTexture.CustomMinimumSize = new Vector2(texture.GetWidth() * 2, 32);
		weaponTexture.Texture = texture;
	}

	public void UpdateAmmoLabel(int currentAmmo, int maxAmmo)
	{
		ammoLabel.Text = $"Rounds: {currentAmmo}/{maxAmmo}";
	}

	public void UpdateSpawnRateLabel(float value)
	{
		spawnRateLabel.Text = $"Current zombie spawn rate: {1/value:0.00} zombies per second";
	}
	
	public void UpdateZombikov(int value)
	{
		zombikov.Text = $"Zombikov: {value}";
	}

	public void SpawnRateVisibility(bool value)
	{
		spawnRateLabel.Visible = value;
	}
}
