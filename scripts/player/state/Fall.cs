using Godot;
using System;

public partial class Fall : State<Player> {
  private Timer cayoteeTimer;
  private bool  cayotee;

  private readonly float CayoteeTime = 0.25f;

  public override void _Ready() {
    cayoteeTimer = new Timer {
      Name     = "CayoteeTimer",
      WaitTime = CayoteeTime,
      OneShot  = true,
    };
    cayoteeTimer.Timeout += () => { cayotee = false; };
    AddChild(cayoteeTimer);
  }

  public override void OnEnter() {
    cayotee = Parent.Jump;
    cayoteeTimer.Start();
  }

  public override void OnProcess(double delta) {
    if (Input.IsActionJustPressed("action_jump") && cayotee) {
      StateMachine.ChangeState("Jump");
      return;
    }
    if (Parent.IsOnFloor()) {
      StateMachine.ChangeState("Move");
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
    if (Parent.Velocity.Y >= 0.0f) {
      Parent.Sprite2D.Play("Fall");
    }
  }
}
