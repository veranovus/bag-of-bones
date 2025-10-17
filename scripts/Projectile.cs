using Godot;
using System;

public interface IDamageable {
  bool TakeDamage(int damage, Vector2 position);
}

public partial class Projectile : Area2D {
  [Export] private int   Damage;
  [Export] private float Speed;

  private Player           player;
  private AnimatedSprite2D sprite2D;
  private Vector2          direction;

  public override void _Ready() {
    player   = (Player)GetTree().GetFirstNodeInGroup("Player");
    sprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    sprite2D.Play("Default");
  }

  public override void _PhysicsProcess(double delta) {
    Position = new Vector2(
      Position.X + direction.X * Speed * (float)delta,
      Position.Y + direction.Y * Speed * (float)delta
    );
  }

  public void SetDamage(int difficulty, int increase) {
    Damage += (difficulty * increase);
  }

  public void SetDirection(Vector2 direction) {
    LookAt(direction);
    this.direction = Vector2.Right.Rotated(Rotation);
  }

  private void OnCollision(Node2D node) {
    if (node is IDamageable damageable) {
      if (!damageable.TakeDamage(Damage, GlobalPosition) && GetCollisionLayerValue(4)) {
        player.AddScore(); 
      }
    }
    QueueFree();
  }

  private void OnScreenExited() {
    QueueFree();
  }
}
