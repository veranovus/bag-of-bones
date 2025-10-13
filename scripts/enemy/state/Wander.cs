using Godot;
using System;

public partial class Wander : State<Enemy> {
  private Vector2 direction = Vector2.Right;

  public override void OnEnter() {
    Parent.Sprite2D.Play("Wander");
  }

  public override void _Process(double delta) {
    SwitchDirection();
  }

  public override void _PhysicsProcess(double delta) {
    Parent.Velocity = Parent.Velocity with { X = Parent.Speed * direction.X };
  }

  private void SwitchDirection() {
    if (!Parent.Raycasts[0].IsColliding() && direction.X < 0.0f) {
      direction = Vector2.Right;
    }
    if (!Parent.Raycasts[1].IsColliding() && direction.X > 0.0f) {
      direction = Vector2.Left;
    }
  }
}
