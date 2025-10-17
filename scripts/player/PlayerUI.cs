using Godot;
using System;

public partial class PlayerUI : CanvasLayer {
  private Player          player;
  private AnimationPlayer animationPlayer;
  private TextureRect     healthbar;
  private TextureRect     specialbar;
  private RichTextLabel   score;
  private RichTextLabel   depth;

  public override void _Ready() {
    player          = (Player)GetTree().GetFirstNodeInGroup("Player");
    animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    healthbar       = GetNode<Node2D>("Healthbar").GetChild<TextureRect>(0);
    specialbar      = GetNode<Node2D>("Specialbar").GetChild<TextureRect>(0);
    score           = GetNode<Node2D>("Score").GetChild<RichTextLabel>(0);
    depth           = GetNode<Node2D>("Depth").GetChild<RichTextLabel>(0);

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
    score.Text    = $"Score: {player.Score}[font_size=32]x{player.Modifier}[/font_size]";
  }

  public void UpdateDepth() {
    depth.Text = $"Depth: {player.Depth:F1}[font_size=32]m[/font_size]";    
  }

  public void PlayAnimation(string animation) {
    animationPlayer.Queue(animation);
  }
}
