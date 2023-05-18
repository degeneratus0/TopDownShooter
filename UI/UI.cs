using Godot;
using System;

public class UI : CanvasLayer
{
    private int score = 0;

    private Label scoreLabel;
    private Label ammoLabel;
    private Label spawnRateLabel;

    public override void _Ready()
    {
        scoreLabel = GetNode<Label>("ScoreLabel");
        ammoLabel = GetNode<Label>("AmmoLabel");
        spawnRateLabel = GetNode<Label>("SpawnRateLabel");
    }

    public void UpdateScoreLabel(int value)
    {
        score+=value;
        scoreLabel.Text = $"Score: {score}";
    }

    public void UpdateAmmoLabel(int currentAmmo, int maxAmmo)
    {
        ammoLabel.Text = $"Rounds: {currentAmmo}/{maxAmmo}";
    }

    public void UpdateSpawnRateLabel(float value)
    {
        spawnRateLabel.Text = $"Current zombie spawn rate: {1/value:0.00} zombies per second";
    }

    public void SpawnRateVisibility(bool value)
    {
        spawnRateLabel.Visible = value;
    }
}
