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
    SetAnimation();
  }

  public override void OnPhysicsProcess(double delta) {
    var input       = Parent.CollectDirectionalInput();
    Parent.Velocity = Parent.Velocity with { X = input.X * Parent.Speed };
  }

  private void SetAnimation() {
    if (Mathf.IsZeroApprox(Parent.Velocity.X)) {
      Parent.Sprite2D.Play("Idle");
    } else {
      Parent.Sprite2D.Play("Move");
    }
  }
}
