using Godot;
using System;

public partial class PlayerUI : CanvasLayer {
  private Player          player;
  private AnimationPlayer animationPlayer;
  private Node2D          healthbar;
  private ShaderMaterial  healthbarMaterial;

  public override void _Ready() {
    player            = (Player)GetTree().GetFirstNodeInGroup("Player");
    animationPlayer   = GetNode<AnimationPlayer>("AnimationPlayer");
    healthbar         = GetNode<Node2D>("Healthbar"); 
    healthbarMaterial = (ShaderMaterial)healthbar.GetChild<TextureRect>(0).Material;

    Visible = true;
  }

  public void UpdateHealthbar() {
    var ratio = player.CurrentHealth / (float)player.Health;
    healthbarMaterial.SetShaderParameter("ratio", ratio);
  }

  public void PlayAnimation(string animation) {
    animationPlayer.Queue(animation);
  }
}
