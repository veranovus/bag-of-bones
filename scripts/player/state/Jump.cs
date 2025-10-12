using Godot;
using System;

public partial class Jump : State<Player> {
  private Timer waitTimer;
  private Timer jumpTimer;
  private bool  wait;
  private bool  jump;

  private const float WaitTime = 0.05f;
  private const float JumpTime = 0.30f;

  public override void _Ready() {
    waitTimer = new Timer {
      Name     = "WaitTimer",
      WaitTime = WaitTime,
      OneShot  = true,
    };
    waitTimer.Timeout += () => { wait = false; };
    AddChild(waitTimer);

    jumpTimer = new Timer {
      Name     = "JumpTimer",
      WaitTime = JumpTime,
      OneShot  = true,
    };
    jumpTimer.Timeout += () => { jump = false; };
    AddChild(jumpTimer);
  }

  public override void OnEnter() {
    wait = true;
    jump = true;

    waitTimer.Start();
    jumpTimer.Start();

    Parent.SetJump(false);
  }

  public override void OnProcess(double delta) {
    if (!Input.IsActionPressed("action_jump")) {
      jump = false;
    }

    if (!wait) {
      if (!jump) {
        StateMachine.ChangeState("Fall");
        return;
      } 
      if (Parent.IsOnFloor()) {
        StateMachine.ChangeState("Move");
        return;
      }
      Parent.CollectAttackInput();
    }
  }

  public override void OnPhysicsProcess(double delta) {
    var input       = Parent.CollectDirectionalInput();
    Parent.Velocity = new Vector2(
      input.X * Parent.Speed,
      -1.0f   * Parent.JumpSpeed
    );
  }
}
