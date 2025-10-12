using Godot;
using System;

public partial class Fall : State<Player> {
  public override void OnProcess(double delta) {
    if (Parent.IsOnFloor()) {
      StateMachine.ChangeState("Move");
    }
  }

  public override void OnPhysicsProcess(double delta) {
    var input       = Parent.CollectDirectionalInput();
    Parent.Velocity = Parent.Velocity with { X = input.X * Parent.Speed };
  }
}
