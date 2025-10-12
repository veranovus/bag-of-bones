using Godot;
using System;

public partial class Move : State<Player> {
  public override void OnProcess(double delta) {
    if (Input.IsActionJustPressed("action_jump")) {
      StateMachine.ChangeState("Jump");
    }
  }

  public override void OnPhysicsProcess(double delta) {
    var input       = Parent.CollectDirectionalInput();
    Parent.Velocity = Parent.Velocity with { X = input.X * Parent.Speed };
  }
}
