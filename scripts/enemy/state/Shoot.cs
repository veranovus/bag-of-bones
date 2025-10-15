using Godot;
using System;

public partial class Shoot : State<Enemy> {
  const float Force = 200.0f;

  public override void OnEnter() {
    SetInitialVelocity();
    PlayShootAnimation();
  }

  private void SetInitialVelocity() {
    Parent.Velocity = -Parent.Direction * Force;
  }

  private void PlayShootAnimation() {
    Parent.AnimationPlayer.Queue("Shoot"); 
  }
}
