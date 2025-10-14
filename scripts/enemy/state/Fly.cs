using Godot;
using System;

public partial class Fly : State<Enemy> {
  private Timer   timer;
  private Vector2 direction;

  private const float TimerTime = 0.2f;

  public override void _Ready() {
    SpawnTimer();
  }

  public override void OnEnter() {
    direction = Vector2.Up;
    timer.Start();

    Parent.Sprite2D.Play("Fly");
  }

  public override void OnPhysicsProcess(double delta) {
    Parent.Velocity = (Parent.Speed / 5.0f) * direction;
  }

  private void SpawnTimer() {
    timer = new Timer {
      Name     = "CayoteeTimer",
      WaitTime = TimerTime,
      OneShot  = true,
    };
    timer.Timeout += PickRandomDirection;
    AddChild(timer);
  }

  private void PickRandomDirection() {
    direction.Y = -direction.Y;
    timer.Start();
  }
}
