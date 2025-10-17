using Godot;
using System;

public partial class EnemySpawner : Marker2D {
  [Export] private PackedScene enemy;

  private Node2D enemyRoot;

  public override void _Ready() {
    enemyRoot = GetTree().GetFirstNodeInGroup("Game").GetNode<Node2D>("Enemies");
    Spawn();
  }

  private void Spawn() {
    var instance      = enemy.Instantiate<Enemy>();
    instance.Position = GlobalPosition;
    enemyRoot.AddChild(instance);
  }
}
