using Godot;
using System;

public partial class PlayerUI : CanvasLayer {
  private Player          player;
  private AnimationPlayer animationPlayer;
  private TextureRect     healthbar;
  private TextureRect     specialbar;
  private RichTextLabel   score;

  public override void _Ready() {
    player           = (Player)GetTree().GetFirstNodeInGroup("Player");
    animationPlayer  = GetNode<AnimationPlayer>("AnimationPlayer");
    healthbar        = GetNode<Node2D>("Healthbar").GetChild<TextureRect>(0);
    specialbar       = GetNode<Node2D>("Specialbar").GetChild<TextureRect>(0);
    score            = GetNode<Node2D>("Score").GetChild<RichTextLabel>(0);

    Visible = true;
  }

  public void UpdateHealthbar() {
    var material = (ShaderMaterial)healthbar.Material;
    var ratio    = player.CurrentHealth / (float)player.Health;
    material.SetShaderParameter("ratio", ratio);
  }

  public void UpdateSpecialbar() {
    var material = (ShaderMaterial)specialbar.Material;
    var ratio    = player.CurrentBone / (float)player.SpecialCharge;
    material.SetShaderParameter("ratio", ratio);
  }

  public void UpdateScore() {
    score.Text = $"Score: {player.Score}";
  }

  public void PlayAnimation(string animation) {
    animationPlayer.Queue(animation);
  }
}
