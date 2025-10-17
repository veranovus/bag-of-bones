using Godot;
using System;

public partial class Fly : State<Enemy> {
  private Timer   timer;
  private Vector2 direction;

  private const float TimerTime      = 0.2f;
  private const float AttackDistance = 1080.0f;

  public override void _Ready() {
    SpawnTimer();
  }

  public override void OnEnter() {
    direction = Vector2.Up;
    timer.Start();

    Parent.Sprite2D.Play("Fly");
  }

  public override void OnExit() {
    timer.Stop();
  }

  public override void OnPhysicsProcess(double delta) {
    Parent.Velocity = (Parent.Speed / 5.0f) * direction;
  }

  private void SpawnTimer() {
    timer = new Timer {
      Name     = "DirectionTimer",
      WaitTime = TimerTime,
      OneShot  = false,
    };
    timer.Timeout += PickRandomDirection;
    AddChild(timer);
  }

  private void PickRandomDirection() {
    direction.Y = -direction.Y;

    var distance = Parent.Player.GlobalPosition - Parent.GlobalPosition;
    if (distance.Length() <= AttackDistance && Parent.CanAttack) {
      Parent.Velocity = Vector2.Zero;
      StateMachine.ChangeState("Shoot"); 
    }
  }
}
