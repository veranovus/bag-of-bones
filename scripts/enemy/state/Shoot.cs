using Godot;
using System;

public partial class Shoot : State<Enemy> {
  public override void OnEnter() {
    PlayShootAnimation();
  }

  private void PlayShootAnimation() {
    Parent.AnimationPlayer.Play("Shoot"); 
  }
}
