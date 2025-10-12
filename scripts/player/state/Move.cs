using Godot;
using System;

public partial class Move : State<Player> {
  public override void OnProcess(double delta) {
    if (Input.IsActionJustPressed("action_jump") && Parent.Jump) {
      StateMachine.ChangeState("Jump");
      return;
    }

    if (Parent.IsOnFloor()) {
      Parent.SetJump(true);
    } else {
      StateMachine.ChangeState("Fall");
      return;
    }

    Parent.CollectAttackInput();
  }

  public override void OnPhysicsProcess(double delta) {
    var input       = Parent.CollectDirectionalInput();
    Parent.Velocity = Parent.Velocity with { X = input.X * Parent.Speed };
  }
}
