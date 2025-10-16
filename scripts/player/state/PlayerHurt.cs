using Godot;
using System;

public partial class PlayerHurt : State<Player> {
  private const float ForceHorizontal = 300.0f;
  private const float ForceVertical   = 600.0f;

  public override void OnEnter() {
    PlayHurtAnimation();
    SetInitialVelocity();
  }

  private void SetInitialVelocity() {
    Parent.Velocity = new Vector2(
      ForceHorizontal * MathF.Sign(Parent.HurtDirection.X), 
      ForceVertical   * ((Parent.HurtDirection.Y >= 0.75) ? 1.0f : -1.0f)
    );
  }

  private void PlayHurtAnimation() {
    Parent.AnimationPlayer.Connect(
      AnimationPlayer.SignalName.AnimationFinished,
      Callable.From<String>((name) => { if(!Parent.Alive) { StateMachine.ChangeState("Death"); } }),
      (uint)ConnectFlags.OneShot
    );
    Parent.AnimationPlayer.Queue("Hurt"); 
  }
}
