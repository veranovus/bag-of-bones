using Godot;
using System;

public partial class Camera : Camera2D {
  private Player player;
  private float  speed;

  private const float InitialSpeed = 64.0f;
  private const float FollowMargin = 1080.0f / 4.0f * 0.5f;

  public override void _Ready() {
    player = (Player)GetTree().GetFirstNodeInGroup("Player");
    speed  = InitialSpeed;
  }

  public override void _PhysicsProcess(double delta) {
    var diff = player.GlobalPosition - GlobalPosition;
    if (diff.Y > FollowMargin) {
      Position = Position with { Y = Position.Y + (diff.Y * 0.9f) * (float)(delta / 0.25) };
    } else {
      Position = Position with { Y = Position.Y + speed * (float)delta };
    }
  }
}
