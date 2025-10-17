using Godot;
using System;

public partial class Wander : State<Enemy> {
  public override void OnEnter() {
    Parent.Sprite2D.Play("Wander");
  }

  public override void OnProcess(double delta) {
    SwitchDirection();
  }

  public override void OnPhysicsProcess(double delta) {
    Parent.Velocity = Parent.Velocity with { X = Parent.Speed * Parent.Direction.X };
  }

  private void SwitchDirection() {
    if (Parent.IsOnFloor()) {
      if (!Parent.Raycasts[0].IsColliding() && Parent.Direction.X < 0.0f) {
        Parent.SetDirection(Vector2.Right);
      }
      if (!Parent.Raycasts[1].IsColliding() && Parent.Direction.X > 0.0f) {
        Parent.SetDirection(Vector2.Left);
      }
      if (Parent.Raycasts[2].IsColliding() && Parent.Direction.X < 0.0f) {
        Parent.SetDirection(Vector2.Right);
      }
      if (Parent.Raycasts[3].IsColliding() && Parent.Direction.X > 0.0f) {
        Parent.SetDirection(Vector2.Left);
      }
    }
  }
}
