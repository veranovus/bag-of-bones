using Godot;
using System;

public partial class EnemySpawner : Marker2D {
  [Export] private PackedScene enemy;

  private static Node2D enemyRoot;

  public override void _Ready() {
    if (enemyRoot == null) {
      CreateEnemyRoot();
    }
    Spawn();
  }

  private void Spawn() {
    var instance      = enemy.Instantiate<Enemy>();
    instance.Position = GlobalPosition;
    enemyRoot.AddChild(instance);
  }
  
  private void CreateEnemyRoot() {
    enemyRoot = new Node2D{ Name = "Enemies" };
    var root  = GetTree().GetFirstNodeInGroup("Game");
    root.AddChild(enemyRoot);

    ConnectOnPlayerDied();
  }

  private void SetEnemyRootToNull() {
    enemyRoot = null;
  }

  private void ConnectOnPlayerDied() {
    var player = (Player)GetTree().GetFirstNodeInGroup("Player");
    player.Connect(
      Player.SignalName.PlayerDied,
      Callable.From(SetEnemyRootToNull),
      (uint)ConnectFlags.OneShot
    );
  }
}
