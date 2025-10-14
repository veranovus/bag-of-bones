using Godot;
using System;

public partial class EnemyHurt : State<Enemy> {
  private const float ForceHorizontal = 300.0f;
  private const float ForceVertical   = 300.0f;

  public override void OnEnter() {
    PlayHurtAnimation();
    SetInitialVelocity();
  }

  private void SetInitialVelocity() {
    Parent.Velocity = new Vector2(
      ForceHorizontal * MathF.Sign(Parent.HurtDirection.X), 
      ForceVertical   * ((Parent.HurtDirection.Y >= 0.75) ? 1.0f : -1.0f)
    );
    Parent.SetDirection(new Vector2(MathF.Sign(Parent.HurtDirection.X), 0.0f));
  }

  private void PlayHurtAnimation() {
    Parent.AnimationPlayer.Connect(
      AnimationPlayer.SignalName.AnimationFinished,
      Callable.From<String>((name) => { if(!Parent.Alive) { Parent.QueueFree(); } }),
      (uint)ConnectFlags.OneShot
    );
    Parent.AnimationPlayer.Play("Hurt"); 
  }
}
