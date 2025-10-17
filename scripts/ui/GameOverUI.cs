using Godot;
using System;

public partial class GameOverUI : CanvasLayer {
  [Export] private PackedScene gameScene;
  [Export] private PackedScene titleScene;

  public override void _Ready() {
    var player = (Player)GetTree().GetFirstNodeInGroup("Player");
    var score  = GetNode<RichTextLabel>("HBoxContainer/CenterMargin/VBoxContainer/TextContainer/VBoxContainer/ScoreLabel");
    var depth  = GetNode<RichTextLabel>("HBoxContainer/CenterMargin/VBoxContainer/TextContainer/VBoxContainer/DepthLabel");

    score.Text = $"Score: {player.Score}";
    depth.Text = $"Depth: {player.Depth:F1}[font_size=32]m[/font_size]";

    GetTree().GetFirstNodeInGroup("Game").CallDeferred("queue_free");
  }

  private void OnTryAgainButtonPressed() {
    var instance = gameScene.Instantiate<Node2D>();
    GetParent().CallDeferred("add_child", instance);
    CallDeferred("queue_free");
  }

  private void OnTitleButtonPressed() {
    var instance = titleScene.Instantiate<CanvasLayer>();
    GetParent().CallDeferred("add_child", instance);
    CallDeferred("queue_free");
  }

  private void OnQuitButtonPressed() {
    GetTree().Quit();
  }
}
