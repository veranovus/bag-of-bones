using Godot;
using System;

public partial class Game : Node2D {
  [Signal] public delegate void DifficultyIncreasedEventHandler(int difficulty);

  private Timer difficultyTimer;

  public int Difficulty { get; private set; }

  private const int   DifficultyMax          = 5;
  private const float DifficultyIncreaseTime = 30.0f;

  public override void _Ready() {
    difficultyTimer = GetNode<Timer>("DifficultyTimer");

    SetDifficultyTimer();
  }

  private void IncreaseDifficulty() {
    if (Difficulty == DifficultyMax) {
      return;
    }
    Difficulty += 1;
    EmitSignal(SignalName.DifficultyIncreased, Difficulty);
    GD.Print($"Difficulty: {Difficulty}");
  }

  private void SetDifficultyTimer() {
    difficultyTimer.WaitTime = DifficultyIncreaseTime;
    difficultyTimer.OneShot  = false;
    difficultyTimer.Timeout += IncreaseDifficulty;
    difficultyTimer.Start();
  }
}
