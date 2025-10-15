using Godot;
using System;

public partial class Jump : State<Player> {
  private Timer waitTimer;
  private Timer jumpTimer;
  private bool  wait;
  private bool  jump;

  private const float WaitTime = 0.05f;
  private const float JumpTime = 0.50f;

  public override void _Ready() {
    SpawnTimers();
  }

  public override void OnEnter() {
    StartTimers();

    Parent.SetJump(false);
    Parent.Sprite2D.Play("Jump");
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

  private void SpawnTimers() {
    Timer spawnTimer(string name, float waitTime) {
      var timer = new Timer {
        Name     = name,
        WaitTime = waitTime,
        OneShot  = true,
      };
      AddChild(timer);
      return timer;
    }

    waitTimer = spawnTimer("WaitTimer", WaitTime);
    waitTimer.Timeout += () => { wait = false; };
    jumpTimer = spawnTimer("JumpTimer", JumpTime);
    jumpTimer.Timeout += () => { jump = false; };
  }

  private void StartTimers() {
    wait = true;
    jump = true;

    waitTimer.Start();
    jumpTimer.Start();
  }
}
