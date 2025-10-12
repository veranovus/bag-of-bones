using Godot;
using System;

public partial class Attack : State<Player> {
  public override void OnEnter() {
    PlayAttackAnimation();
  }

  private void PlayAttackAnimation() {
    Parent.AnimationPlayer.Play(Parent.Attack);
    Parent.AnimationPlayer.Connect(
      AnimationPlayer.SignalName.AnimationFinished,
      Callable.From<String>((name) => { StateMachine.ChangeState("Move"); }),
      (uint)ConnectFlags.OneShot
    );
  }
}
