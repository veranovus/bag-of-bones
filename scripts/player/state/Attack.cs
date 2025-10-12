using Godot;
using System;

public partial class Attack : State<Player> {
  public override void OnEnter() {
    Parent.AnimationPlayer.Play(Parent.Attack);
    Parent.AnimationPlayer.Connect(
      AnimationPlayer.SignalName.AnimationFinished,
      Callable.From<String>((name) => { StateMachine.ChangeState("Move"); }),
      (uint)ConnectFlags.OneShot
    );
  }

  public override void OnPhysicsProcess(double delta) {
    var input       = Parent.CollectDirectionalInput();
    Parent.Velocity = Parent.Velocity with { X = input.X * Parent.Speed };
  }
}
